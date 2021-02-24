using System;
using System.Threading.Tasks;
using GemstarPaymentCore.Business.BusinessHandlers.ChinaumsPay;
using GemstarPaymentCore.Business.Utility;
using GemstarPaymentCore.Data;
using GemstarPaymentCore.Payment.ChinaumsPay;
using GemstarPaymentCore.Payment.ChinaumsPay.Request;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace GemstarPaymentCore.Business.BusinessQuery
{
    /// <summary>
    /// 查询银商支付码支付结果
    /// </summary>
    [DisallowConcurrentExecution]
    public class ChinaumsPayQueryJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            //获取参数
            var serviceProvider = context.Scheduler.Context.Get(JobParaName.ParaServiceProviderName) as IServiceProvider;
            var jobDataMap = context.JobDetail.JobDataMap;
            var systemName = jobDataMap.GetString(JobParaName.ParaSystemName);
            var connStr = jobDataMap.GetString(JobParaName.ParaConnStrName);
            var log = serviceProvider.GetService<ILogger<ChinaumsPayQueryJob>>();
            try
            {
                using (var scope = log.BeginScope(this))
                {
                    //从发送表中读取要发送的数据
                    var dbContextOption = new DbContextOptionsBuilder<WxPayDB>();
                    dbContextOption.UseSqlServer(connStr);
                    using (var payDB = new WxPayDB(dbContextOption.Options))
                    {
                        var cyUserInfo = await payDB.CYUserInfos.FirstAsync();
                        var security = serviceProvider.GetService<ISecurity>();
                        var businessOption = serviceProvider.GetService<IOptions<BusinessOption>>().Value;
                        var chinaumsOption = serviceProvider.GetService<IOptions<ChinaumsPayOption>>().Value;
                        var records = WxPayInfoHelper.GetChinaumsNeedQueryStatus(payDB, businessOption);
                        if (records.Count > 0)
                        {
                            foreach (var record in records)
                            {
                                try
                                {

                                    log.LogInformation($"开始查询银商支付状态：{record.ID}");

                                    var queryRequest = new ChinaumsPayQueryRequest
                                    {
                                        BillDate = record.BuildDate.Value.ToString("yyyy-MM-dd"),
                                        BillNo = record.ID,
                                        MsgSrc = chinaumsOption.MsgSrc,
                                    };

                                    var client = serviceProvider.GetService<IChinaumsPayClient>();
                                    var response = await client.ExecuteRequestAsync(queryRequest, chinaumsOption);
                                    var result = HandleResult.Fail("未知错误");
                                    if (response != null && response.IsSuccessCode())
                                    {
                                        var tradeStatus = response.BillStatus;
                                        if (tradeStatus == "PAID")
                                        {
                                            var tradeNo = response.BillPayment.PaySeqId;
                                            var paidTime = DateTime.Parse(response.BillPayment.PayTime);
                                            var paidAmount = response.BillPayment.InvoiceAmount / 100.0;
                                            var payType = response.BillPayment.TargetSys;
                                            WxPayInfoHelper.ChinaumsPaidSuccess(payDB, record, tradeNo, paidTime, paidAmount.Value,payType);
                                        } else if (tradeStatus == "REFUND" || tradeStatus == "TRADE_REFUND" || tradeStatus == "CLOSED")
                                        {
                                            WxPayInfoHelper.ChinaumsPaidFail(payDB, record, "未付款交易超时关闭，或支付完成后全额退款");
                                        }
                                    } else
                                    {
                                        //通信失败，直接记录失败原因到日志里面
                                        log.LogError($"银商查询失败：{(response == null ? "没有收到返回内容" : "状态：" + response.ErrCode + response.ErrMsg)}");
                                    }
                                } catch (Exception ex)
                                {
                                    log.LogError(ex, $"银商支付结果查询遇到错误：{ex.Message}");
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                using (var scope = log.BeginScope(this))
                {
                    log.LogError(ex, $"银商支付结果查询遇到错误：{ex.Message}");
                }
            }
        }
    }
}
