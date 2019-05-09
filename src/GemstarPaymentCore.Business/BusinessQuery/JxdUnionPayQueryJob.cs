using GemstarPaymentCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessQuery
{
    /// <summary>
    /// 捷信达聚合支付扫码结果查询
    /// </summary>
    public class JxdUnionPayQueryJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            //获取参数
            var serviceProviderRoot = context.Scheduler.Context.Get(JobParaName.ParaServiceProviderName) as IServiceProvider;
            var serviceProvider = serviceProviderRoot.CreateScope().ServiceProvider;
            var jobDataMap = context.JobDetail.JobDataMap;
            var systemName = jobDataMap.GetString(JobParaName.ParaSystemName);
            var connStr = jobDataMap.GetString(JobParaName.ParaConnStrName);
            var log = serviceProvider.GetService<ILogger<JxdUnionPayQueryJob>>();
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
                        var records = WxPayInfoHelper.GetJxdUnionPayOrderNeedStatus(payDB, businessOption);
                        if (records.Count > 0)
                        {
                            //计算捷信达线上的查询地址
                            var uriBase = new Uri(businessOption.JxdPaymentUrl);
                            var queryPath = "/JxdUnionPay/QueryPayResult";
                            var queryUri = new Uri(uriBase, queryPath).AbsoluteUri;
                            var httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient();
                            foreach (var record in records)
                            {
                                try
                                {
                                    log.LogInformation($"开始查询捷信达聚合支付状态：{record.ID}");
                                    using (var requestContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>> {
                                        KeyValuePair.Create("terminalTrace",record.ID)
                                    }))
                                    using (var response = await httpClient.PostAsync(queryUri, requestContent))
                                    using (var responseContent = response.Content)
                                    {
                                        var resultStr = await responseContent.ReadAsStringAsync();
                                        log.LogInformation($"收到的查询捷信达聚合支付状态返回消息：{resultStr}");
                                        var payResult = JsonConvert.DeserializeObject<JxdUnionPayResult>(resultStr);
                                        if (payResult.PaySuccess)
                                        {
                                            //支付成功
                                            var paidTime = payResult.PaidTime;
                                            var transactionId = payResult.PaidTransId;
                                            var paidAmount = payResult.PaidAmount;

                                            WxPayInfoHelper.JxdUnionPayPaidSuccess(payDB, record, transactionId, paidTime, paidAmount);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.LogError(ex, $"查询捷信达聚合支付订单时遇到异常{ex.Message}");
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
                    log.LogError(ex, $"查询捷信达聚合支付订单时遇到异常{ex.Message}");
                }
            }
        }
    }
}
