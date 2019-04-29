using Essensoft.AspNetCore.Payment.Alipay;
using Essensoft.AspNetCore.Payment.Alipay.Domain;
using Essensoft.AspNetCore.Payment.Alipay.Request;
using GemstarPaymentCore.Business.BusinessHandlers.Alipay;
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
    /// 查询支付宝扫码支付结果
    /// </summary>
    public class AlipayQueryJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            //获取参数
            var serviceProvider = context.Scheduler.Context.Get(JobParaName.ParaServiceProviderName) as IServiceProvider;
            var jobDataMap = context.JobDetail.JobDataMap;
            var systemName = jobDataMap.GetString(JobParaName.ParaSystemName);
            var connStr = jobDataMap.GetString(JobParaName.ParaConnStrName);
            var log = serviceProvider.GetService<ILogger<AlipayQueryJob>>();
            try
            {
                using (var scope = log.BeginScope(this))
                {
                    //从发送表中读取要发送的数据
                    var dbContextOption = new DbContextOptionsBuilder<WxPayDB>();
                    dbContextOption.UseSqlServer(connStr);
                    using (var payDB = new WxPayDB(dbContextOption.Options))
                    {
                        var businessOption = serviceProvider.GetService<IOptions<BusinessOption>>().Value;
                        var alipayOption = serviceProvider.GetService<IOptions<AlipayOptions>>().Value;
                        var records = WxPayInfoHelper.GetNeedQueryStatus(payDB, businessOption);
                        if (records.Count > 0)
                        {
                            foreach (var record in records)
                            {
                                try
                                {

                                    log.LogInformation($"开始查询支付状态：{record.ID}");

                                    var appId = record.AppID;
                                    //var pid = record.MchID;
                                    if ("zfb".Equals(appId, StringComparison.OrdinalIgnoreCase))
                                    {
                                        appId = alipayOption.AppId;
                                    }
                                    //if ("zfb".Equals(pid, StringComparison.OrdinalIgnoreCase))
                                    //{
                                    //    pid = Settings.PID;
                                    //}
                                    var queryModel = new AlipayTradeQueryModel
                                    {
                                        OutTradeNo = record.ID
                                    };
                                    var queryRequest = new AlipayTradeQueryRequest();
                                    queryRequest.SetBizModel(queryModel);
                                    var client = serviceProvider.GetService<IAlipayClient>();
                                    var response = await client.ExecuteAsync(queryRequest);
                                    var result = HandleResult.Fail("未知错误");
                                    if (response != null && response.Code == ResultCode.SUCCESS)
                                    {
                                        var tradeStatus = response.TradeStatus;
                                        if (tradeStatus == "TRADE_SUCCESS" || tradeStatus == "TRADE_FINISHED")
                                        {
                                            var tradeNo = response.TradeNo;
                                            var paidTime = DateTime.Parse(response.SendPayDate);
                                            var paidAmount = double.Parse(response.ReceiptAmount);
                                            WxPayInfoHelper.AlipayPaidSuccess(payDB, record, tradeNo, paidTime, paidAmount);
                                        }
                                        else if (tradeStatus == "TRADE_CLOSED")
                                        {
                                            WxPayInfoHelper.AlipayPaidFail(payDB, record, "未付款交易超时关闭，或支付完成后全额退款");
                                        }
                                    }
                                    else
                                    {
                                        //通信失败，直接记录失败原因到日志里面
                                        log.LogError($"查询失败：{(response == null ? "没有收到返回内容" : "状态：" + response.Code + response.Msg)}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.LogError(ex,$"支付宝支付结果查询遇到错误：{ex.Message}");
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                using (var scope = log.BeginScope(this))
                {
                    log.LogError(ex,$"支付宝支付结果查询遇到错误：{ex.Message}");
                }
            } 
        }
    }
}
