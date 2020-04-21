using System.Threading;
using System.Threading.Tasks;
using GemstarPaymentCore.Business;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System.Linq;
using GemstarPaymentCore.Business.BusinessQuery;

namespace GemstarPaymentCore.Models
{
    public class TransferDataToHistoryService : IHostedService
    {
        private ILogger _logger;
        private BusinessOption _businessOption;
        private const string DeployOnline = "online";
        private IScheduler _scheduler;
        public TransferDataToHistoryService(ILogger<TransferDataToHistoryService> logger,IOptions<BusinessOption> businessOption)
        {
            _logger = logger;
            _businessOption = businessOption.Value;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_businessOption.Deploy.Equals(DeployOnline, System.StringComparison.OrdinalIgnoreCase))
            {
                var dbs = _businessOption.Systems.Where(w => w.HavePay == 1).ToList();
                if (dbs.Count > 0)
                {
                    var schedulerBuilder = new Quartz.Impl.StdSchedulerFactory();
                    _scheduler = schedulerBuilder.GetScheduler().Result;

                    foreach(var db in dbs)
                    {
                        var job = JobBuilder.Create<TransferDataToHistoryJob>()
                            .WithIdentity(db.Name)
                            .Build();
                        job.JobDataMap.Put(JobParaName.ParaSystemName, db.Name);
                        job.JobDataMap.Put(JobParaName.ParaConnStrName, db.ConnStr);

                        var trigger = TriggerBuilder.Create()
                            .WithIdentity(db.Name)
                            .WithDailyTimeIntervalSchedule(s=>s.OnEveryDay().StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(01,0)).EndingDailyAt(TimeOfDay.HourAndMinuteOfDay(05,0)).WithIntervalInHours(24))
                            .Build();
                        _scheduler.ScheduleJob(job, trigger, cancellationToken);
                    }
                    _scheduler.Start(cancellationToken);
                    _logger.LogInformation("转移数据服务已经启动");
                }
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_businessOption.Deploy.Equals(DeployOnline, System.StringComparison.OrdinalIgnoreCase))
            {
                if(_scheduler != null)
                {
                    _scheduler.Shutdown(cancellationToken);
                }
                _logger.LogInformation("转移数据服务已经停止");
            }
            return Task.CompletedTask;
        }
    }
}
