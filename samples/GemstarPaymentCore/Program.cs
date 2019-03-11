using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Gemstar.Extensions.Logging.AliyunLogService;
using System;
using Microsoft.Extensions.Options;
using GemstarPaymentCore.Business;
using GemstarPaymentCore.Business.BusinessQuery;
using Quartz;
using Microsoft.Extensions.DependencyInjection;

namespace GemstarPaymentCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            //开始业务扫描
            var serviceProvider = host.Services.GetService<IServiceProvider>();
            StartBusinessScanJobs(serviceProvider);
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureLogging(builder=> {
                builder.ClearProviders();
                builder.AddConsole();
                //由于此程序可能会部署在用户现场，不想让用户现场的人知道我们阿里云的访问密钥，所以先直接写死在代码里面
                var aliyunLoggerOption = new AliyunLogOptions {
                    AccessKey = "LTAI0JA2a0keVkuB",
                    AccessSecret = "tNP2M2jbqdmXPrRM4IZBd5IFVnfSHq",
                    Endpoint = "cn-shenzhen.log.aliyuncs.com",
                    ProjectName = "jxd-payment-logs",
                    LogStoreName = "wechat-payment",
                    SourceName = "GemstarPayment",
                    LogLevel = LogLevel.Information
                };
                builder.AddProvider(new AliyunLoggerProvider(aliyunLoggerOption));
            })
            .UseKestrel(config => {
                config.ListenAnyIP(8999);
            })
                .UseStartup<Startup>();

        private async static void StartBusinessScanJobs(IServiceProvider serviceProvider)
        {
            bool hasJob = false;
            var businessOption = serviceProvider.GetService<IOptions<BusinessOption>>().Value;
            foreach (var system in businessOption.Systems)
            {
                if (system.HavePay == 1 && system.NeedQuery == 1)
                {
                    if (_scheduler == null)
                    {
                        var schedulerFactory = new Quartz.Impl.StdSchedulerFactory();
                        _scheduler = await schedulerFactory.GetScheduler();
                        _scheduler.Context.Put(JobParaName.ParaServiceProviderName, serviceProvider);
                    }
                    var job = JobBuilder.Create<WxProviderQueryJob>()
                        .WithIdentity($"wxquery{system.Name}", "wxquery")
                        .Build();
                    job.JobDataMap.Put(JobParaName.ParaSystemName, system.Name);
                    job.JobDataMap.Put(JobParaName.ParaConnStrName, system.ConnStr);

                    var trigger = TriggerBuilder.Create()                        
                        .WithIdentity($"wxquery{system.Name}", "wxquery")
                        .WithSimpleSchedule(s=>s.WithIntervalInSeconds(businessOption.QueryInterval).RepeatForever())
                        .Build();
                    await _scheduler.ScheduleJob(job, trigger);

                    hasJob = true;
                }
            }
            if (hasJob)
            {
                await _scheduler.Start();
            }
        }
        private static IScheduler _scheduler;
    }
}
