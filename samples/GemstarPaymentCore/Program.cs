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
using System.Diagnostics;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace GemstarPaymentCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            var builder = CreateWebHostBuilder(
                args.Where(arg => arg != "--console").ToArray());

            var host = builder.Build();

            //开始业务扫描
            var serviceProvider = host.Services.GetService<IServiceProvider>();
            StartBusinessScanJobs(serviceProvider);
            if (isService)
            {
                // To run the app without the CustomWebHostService change the
                // next line to host.RunAsService();
                host.RunAsService();
            }
            else
            {
                host.Run();
            }

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
                    var jobWxquery = JobBuilder.Create<WxProviderQueryJob>()
                        .WithIdentity($"wxquery{system.Name}", "wxquery")
                        .Build();
                    jobWxquery.JobDataMap.Put(JobParaName.ParaSystemName, system.Name);
                    jobWxquery.JobDataMap.Put(JobParaName.ParaConnStrName, system.ConnStr);


                    var triggerWxquery = TriggerBuilder.Create()                        
                        .WithIdentity($"wxquery{system.Name}", "wxquery")
                        .WithSimpleSchedule(s=>s.WithIntervalInSeconds(businessOption.QueryInterval).RepeatForever())
                        .Build();
                    await _scheduler.ScheduleJob(jobWxquery, triggerWxquery);

                    var jobAliquery = JobBuilder.Create<AlipayQueryJob>()
                        .WithIdentity($"aliquery{system.Name}", "aliquery")
                        .Build();
                    jobAliquery.JobDataMap.Put(JobParaName.ParaSystemName, system.Name);
                    jobAliquery.JobDataMap.Put(JobParaName.ParaConnStrName, system.ConnStr);

                    var triggerAliquery = TriggerBuilder.Create()
                        .WithIdentity($"aliquery{system.Name}", "aliquery")
                        .WithSimpleSchedule(s=>s.WithIntervalInSeconds(businessOption.QueryInterval).RepeatForever())
                        .Build();
                    await _scheduler.ScheduleJob(jobAliquery,triggerAliquery);

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
