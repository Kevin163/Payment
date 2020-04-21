using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GemstarPaymentCore.Business;
using GemstarPaymentCore.Business.BusinessQuery;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace GemstarPaymentCore.Models
{
    /// <summary>
    /// 查询支付结果后台服务
    /// </summary>
    public class QueryPayResultService : IHostedService
    {
        private BusinessOption _businessOption;
        private ILogger _logger;
        private IScheduler _scheduler;
        private IServiceProvider _serviceProvider;
        public QueryPayResultService(ILogger<QueryPayResultService> logger,IOptions<BusinessOption> option,IServiceProvider serviceProvider)
        {
            _logger = logger;
            _businessOption = option.Value;
            _serviceProvider = serviceProvider;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var needQuerySystems = _businessOption.Systems.Where(w => w.HavePay == 1 && w.NeedQuery == 1).ToList();
            if (needQuerySystems.Count > 0 && _businessOption.Deploy.Equals("local",StringComparison.OrdinalIgnoreCase))
            {
                var schedulerFactory = new Quartz.Impl.StdSchedulerFactory();
                _scheduler = schedulerFactory.GetScheduler().Result;
                _scheduler.Context.Put(JobParaName.ParaServiceProviderName, _serviceProvider);

                var tasks = new List<Task>();

                foreach (var system in needQuerySystems)
                {
                    //启动微信服务商扫码结果查询
                    var jobWxquery = JobBuilder.Create<WxProviderQueryJob>()
                        .WithIdentity($"wxquery{system.Name}", "wxquery")
                        .Build();
                    jobWxquery.JobDataMap.Put(JobParaName.ParaSystemName, system.Name);
                    jobWxquery.JobDataMap.Put(JobParaName.ParaConnStrName, system.ConnStr);


                    var triggerWxquery = TriggerBuilder.Create()
                        .WithIdentity($"wxquery{system.Name}", "wxquery")
                        .WithSimpleSchedule(s => s.WithIntervalInSeconds(_businessOption.QueryInterval).RepeatForever())
                        .Build();
                    tasks.Add(_scheduler.ScheduleJob(jobWxquery, triggerWxquery));
                    //启动支付宝扫码支付结果查询
                    var jobAliquery = JobBuilder.Create<AlipayQueryJob>()
                        .WithIdentity($"aliquery{system.Name}", "aliquery")
                        .Build();
                    jobAliquery.JobDataMap.Put(JobParaName.ParaSystemName, system.Name);
                    jobAliquery.JobDataMap.Put(JobParaName.ParaConnStrName, system.ConnStr);

                    var triggerAliquery = TriggerBuilder.Create()
                        .WithIdentity($"aliquery{system.Name}", "aliquery")
                        .WithSimpleSchedule(s => s.WithIntervalInSeconds(_businessOption.QueryInterval).RepeatForever())
                        .Build();
                    tasks.Add(_scheduler.ScheduleJob(jobAliquery, triggerAliquery));
                    //启动扫呗支付扫码支付结果查询
                    var jobLcswPayQuery = JobBuilder.Create<LcswPayQueryJob>()
                       .WithIdentity($"lcswpayquery{system.Name}", "lcswpayquery")
                       .Build();
                    jobLcswPayQuery.JobDataMap.Put(JobParaName.ParaSystemName, system.Name);
                    jobLcswPayQuery.JobDataMap.Put(JobParaName.ParaConnStrName, system.ConnStr);

                    var triggerlcswpayquery = TriggerBuilder.Create()
                        .WithIdentity($"lcswpayquery{system.Name}", "lcswpayquery")
                        .WithSimpleSchedule(s => s.WithIntervalInSeconds(_businessOption.QueryInterval).RepeatForever())
                        .Build();
                    tasks.Add(_scheduler.ScheduleJob(jobLcswPayQuery, triggerlcswpayquery));
                    //启动扫呗支付扫码支付结果查询
                    var jobJxdUnionPayQuery = JobBuilder.Create<JxdUnionPayQueryJob>()
                       .WithIdentity($"jxdunionpayquery{system.Name}", "jxdunionpayquery")
                       .Build();
                    jobJxdUnionPayQuery.JobDataMap.Put(JobParaName.ParaSystemName, system.Name);
                    jobJxdUnionPayQuery.JobDataMap.Put(JobParaName.ParaConnStrName, system.ConnStr);

                    var triggerjxdunionpayquery = TriggerBuilder.Create()
                        .WithIdentity($"jxdunionpayquery{system.Name}", "jxdunionpayquery")
                        .WithSimpleSchedule(s => s.WithIntervalInSeconds(_businessOption.QueryInterval).RepeatForever())
                        .Build();
                    tasks.Add(_scheduler.ScheduleJob(jobJxdUnionPayQuery, triggerjxdunionpayquery));
                    //启动退款任务
                    var refundJob = JobBuilder.Create<WaitRefundJob>()
                        .WithIdentity($"WaitRefundJob{system.Name}", "WaitRefundJob")
                        .Build();
                    refundJob.JobDataMap.Put(JobParaName.ParaSystemName, system.Name);
                    refundJob.JobDataMap.Put(JobParaName.ParaConnStrName, system.ConnStr);

                    var triggerRefundJob = TriggerBuilder.Create()
                        .WithIdentity($"WaitRefundJob{system.Name}", "WaitRefundJob")
                        .WithSimpleSchedule(s => s.WithIntervalInSeconds(_businessOption.QueryInterval).RepeatForever())
                        .Build();

                    tasks.Add(_scheduler.ScheduleJob(refundJob, triggerRefundJob));

                    Task.WaitAll(tasks.ToArray());
                }
                _scheduler.Start();
                _logger.LogInformation("查询本地支付结果后台任务已经启动");
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if(_scheduler != null)
            {
                _scheduler.Shutdown();
                _logger.LogInformation("查询本地支付结果后台任务已经退出");
            }
            return Task.CompletedTask;
        }
    }
}
