using Essensoft.AspNetCore.Payment.LcswPay;
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
    /// 待退款记录执行退款作业
    /// </summary>
    public class WaitRefundJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            //获取参数
            var serviceProviderRoot = context.Scheduler.Context.Get(JobParaName.ParaServiceProviderName) as IServiceProvider;
            var serviceProvider = serviceProviderRoot.CreateScope().ServiceProvider;
            var jobDataMap = context.JobDetail.JobDataMap;
            var systemName = jobDataMap.GetString(JobParaName.ParaSystemName);
            var connStr = jobDataMap.GetString(JobParaName.ParaConnStrName);
            var log = serviceProvider.GetService<ILogger<WaitRefundJob>>();
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
                        var records = WxPayInfoHelper.GetNotSendWaitRefunds(payDB);
                        if (records.Count > 0)
                        {
                            foreach (var record in records)
                            {
                                try
                                {
                                    log.LogInformation($"开始处理退款：{record.RefundId},支付方式:{record.PayType}");
                                    //记录退款请求发送时间
                                    record.SendDate = DateTime.Now;
                                    record.RefundStatus = WaitRefundList.RefundStatu.StatuSended;
                                    payDB.Entry(record).State = EntityState.Modified;
                                    await payDB.SaveChangesAsync();
                                    //执行退款
                                    var refundInstance = DoRefundBase.GetDoRefundInstance(record.PayType);
                                    var refundResult = await refundInstance.DoRefund(record, serviceProvider);
                                    //记录退款状态
                                    record.RefundStatus = refundResult.RefundStatu;
                                    record.RefundFailReason = refundResult.RefundFailReason;
                                    payDB.Entry(record).State = EntityState.Modified;
                                    await payDB.SaveChangesAsync();
                                }
                                catch (Exception ex)
                                {
                                    log.LogError(ex, $"退款时遇到异常{ex.Message}");
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
                    log.LogError(ex, $"退款时遇到异常{ex.Message}");
                }
            }
        }

    }
}
