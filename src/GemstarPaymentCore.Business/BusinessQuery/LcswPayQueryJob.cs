using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
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
    /// 利楚商务扫呗支付扫码结果查询
    /// </summary>
    public class LcswPayQueryJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            //获取参数
            var serviceProvider = context.Scheduler.Context.Get(JobParaName.ParaServiceProviderName) as IServiceProvider;
            var jobDataMap = context.JobDetail.JobDataMap;
            var systemName = jobDataMap.GetString(JobParaName.ParaSystemName);
            var connStr = jobDataMap.GetString(JobParaName.ParaConnStrName);
            var log = serviceProvider.GetService<ILogger<LcswPayQueryJob>>();
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
                        var options = serviceProvider.GetService<IOptions<LcswPayOption>>().Value;
                        var client = serviceProvider.GetService<ILcswPayClient>();
                        var records = WxPayInfoHelper.GetWxProviderOrderNeedStatus(payDB, businessOption);
                        if (records.Count > 0)
                        {
                            foreach (var record in records)
                            {
                                try
                                {
                                    log.LogInformation($"开始查询利楚商务扫呗支付状态：{record.ID}");
                                    //调用查询接口
                                    var request = new LcswPayQueryRequest
                                    {
                                        PayType = "000",
                                        ServiceId = "020",
                                        MerchantNo = record.MchID,//要求扫呗支付时，将商户号写入到wxpayinfo表中的MchID
                                        TerminalId = record.AppID,//要求扫呗支付时，将终端号写入到wxpayinfo表中的AppID
                                        TerminalTime = DateTime.Now.ToString("yyyyMMddHHmmss"),
                                        TerminalTrace = Guid.NewGuid().ToString("N"),//流水号先使用不重复的guid值，后续是不需要此值的
                                        PayTrace = record.ID,
                                        PayTime = record.BuildDate.Value.ToString("yyyyMMddHHmmss"),
                                        OutTradeNo = record.PrePayID
                                    };
                                    options.Token = record.Key;//要求扫呗支付时，将令牌写入到wxpayinfo表中的appkey
                                    var response = await client.ExecuteAsync(request, options);
                                    log.LogInformation($"收到的查询利楚商务扫呗支付状态返回消息：{response.Body}");
                                    if (response.IsReturnCodeSuccess)
                                    {
                                        if (response.ResultCode == "01" && response.TradeState == "SUCCESS")
                                        {
                                            //支付成功
                                            var paidTimeStr = response.EndTime;
                                            var paidTime = DateTime.ParseExact(paidTimeStr, "yyyyMMddHHmmss", null);
                                            var transactionId = response.OutTradeNo;
                                            var paidAmount = Convert.ToInt32(response.TotalFee);

                                            WxPayInfoHelper.LcswPayPaidSuccess(payDB, record, transactionId, paidTime, paidAmount);

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.LogError(ex, $"查询利楚商务扫呗支付订单时遇到异常{ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (var scope = log.BeginScope(this))
                {
                    log.LogError(ex, $"查询利楚商务扫呗支付订单时遇到异常{ex.Message}");
                }
            }
        }
    }
}
