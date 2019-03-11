using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using GemstarPaymentCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessQuery
{
    /// <summary>
    /// 查询微信服务商支付结果
    /// </summary>
    public class WxProviderQueryJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            //获取参数
            var serviceProvider = context.Scheduler.Context.Get(JobParaName.ParaServiceProviderName) as IServiceProvider;
            var jobDataMap = context.JobDetail.JobDataMap;
            var systemName = jobDataMap.GetString(JobParaName.ParaSystemName);
            var connStr = jobDataMap.GetString(JobParaName.ParaConnStrName);
            var log = serviceProvider.GetService<ILogger<WxProviderQueryJob>>();
            try
            {
                //从发送表中读取要发送的数据
                var dbContextOption = new DbContextOptionsBuilder<WxPayDB>();
                dbContextOption.UseSqlServer(connStr);
                using (var payDB = new WxPayDB(dbContextOption.Options))
                {
                    var businessOption = serviceProvider.GetService<IOptions<BusinessOption>>().Value;
                    var records = WxPayInfoHelper.GetWxProviderOrderNeedStatus(payDB, businessOption);
                    if (records.Count > 0)
                    {
                        foreach (var record in records)
                        {
                            try
                            {
                                log.LogInformation("WxProviderQueryJob", "开始查询微信服务商支付状态：" + record.ID);
                                //开始查询
                                var request = new WeChatPayOrderQueryRequest
                                {
                                    OutTradeNo = record.ID,
                                    SubAppId = record.AppID,
                                    SubMchId = record.MchID
                                };
                                var client = serviceProvider.GetService<IWeChatPayClient>();
                                var result = await client.ExecuteAsync(request);
                                log.LogInformation("WxProviderQueryJob", "收到的查询微信服务商支付状态返回消息：" + result.Body);
                                //根据返回消息进行处理
                                if (result.ReturnCode == "SUCCESS")
                                {
                                    //通信成功，则判断业务结果
                                    if (result.ResultCode == "SUCCESS")
                                    {
                                        //业务结果返回成功，判断支付状态
                                        var tradeState = result.TradeState;
                                        if (tradeState == "SUCCESS")
                                        {
                                            //支付成功
                                            var paidTimeStr = result.TimeEnd;
                                            var paidTime = DateTime.ParseExact(paidTimeStr, "yyyyMMddHHmmss", null);
                                            var transactionId = result.TransactionId;
                                            var paidAmount = Convert.ToInt32(result.TotalFee);

                                            WxPayInfoHelper.WxProviderPaidSuccess(payDB, record, transactionId, paidTime, paidAmount);
                                        } else if (tradeState == "USERPAYING" || tradeState == "NOTPAY" || tradeState == "SYSTEMERROR" || tradeState == "BANKERROR")
                                        {
                                            //用户支付中
                                            //NOTPAY是指打印出了二维码，但客人还没有扫描，但接下来客人有可能会继续扫的，所以等下继续查询状态
                                            //不再任何处理，等下再次查询即可
                                        } else
                                        {
                                            //支付失败
                                            var errMsg = string.Format("支付失败状态:{0},描述:{1}", tradeState, result.TradeStateDesc);
                                            WxPayInfoHelper.WxProviderPaidFail(payDB, record, errMsg);
                                        }
                                    } else
                                    {
                                        //业务查询失败，记录失败原因到日志里面
                                        log.LogError("WxProviderQueryJob", string.Format("查询微信服务商订单状态时遇到业务错误,代码:{0},描述:{1}", result.ErrCode, result.ErrCodeDes));
                                    }
                                } else
                                {
                                    //通信失败，直接记录失败原因到日志里面
                                    log.LogError("WxProviderQueryJob", string.Format("查询微信服务商订单状态时遇到通信错误:{0}", result.ReturnMsg));
                                }
                            } catch (Exception ex)
                            {
                                log.LogError("WxProviderQueryJob", ex);
                            } finally
                            {
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                log.LogError("WxProviderQueryJob", ex);
            } 
        }
    }
}
