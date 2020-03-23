﻿using Microsoft.AspNetCore;
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
using NLog.Web;
using Microsoft.Extensions.Configuration;

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
                args.Where(arg => arg != "--console").ToArray(),isService);

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
                var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
                try
                {
                    logger.Debug("init main");
                    host.Run();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Stopped program because of exception");
                    throw;
                }
                finally
                {
                    NLog.LogManager.Shutdown();
                }
            }
            if(_scheduler != null && !_scheduler.IsShutdown)
            {
                _scheduler.Shutdown().Wait();
                Console.WriteLine("定时任务已经停止");
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, bool isService)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var portStr = config["Port"];
            int port = 8999;
            if(string.IsNullOrEmpty(portStr) || !int.TryParse(portStr,out port))
            {
                port = 8999;
            }
            var hostBuilder = WebHost.CreateDefaultBuilder(args)
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders();
                //如果是控制台运行的，则添加控制台日志记录
                if (!isService)
                {
                    builder.AddConsole();
                }
                else
                {
                    //由于此程序可能会部署在用户现场，不想让用户现场的人知道我们阿里云的访问密钥，所以先直接写死在代码里面
                    var aliyunLoggerOption = new AliyunLogOptions
                    {
                        AccessKey = "LTAI0JA2a0keVkuB",
                        AccessSecret = "tNP2M2jbqdmXPrRM4IZBd5IFVnfSHq",
                        Endpoint = "cn-shenzhen.log.aliyuncs.com",
                        ProjectName = "jxd-payment-logs",
                        LogStoreName = "wechat-payment",
                        SourceName = "GemstarPayment",
                        LogLevel = LogLevel.Information
                    };
                    builder.AddProvider(new AliyunLoggerProvider(aliyunLoggerOption));
                }
            })
            .UseKestrel(cfg =>
            {
                cfg.ListenAnyIP(port);
            })
            .UseStartup<Startup>();
            //如果是使用控制台运行的，则将日志文件写到文件中，以便分析日志
            if (!isService)
            {
                hostBuilder.UseNLog();
            }
            return hostBuilder;
        }

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
                    //启动微信服务商扫码结果查询
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
                    //启动支付宝扫码支付结果查询
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
                    //启动扫呗支付扫码支付结果查询
                    var jobLcswPayQuery = JobBuilder.Create<LcswPayQueryJob>()
                       .WithIdentity($"lcswpayquery{system.Name}", "lcswpayquery")
                       .Build();
                    jobLcswPayQuery.JobDataMap.Put(JobParaName.ParaSystemName, system.Name);
                    jobLcswPayQuery.JobDataMap.Put(JobParaName.ParaConnStrName, system.ConnStr);

                    var triggerlcswpayquery = TriggerBuilder.Create()
                        .WithIdentity($"lcswpayquery{system.Name}", "lcswpayquery")
                        .WithSimpleSchedule(s => s.WithIntervalInSeconds(businessOption.QueryInterval).RepeatForever())
                        .Build();
                    await _scheduler.ScheduleJob(jobLcswPayQuery, triggerlcswpayquery);
                    //启动扫呗支付扫码支付结果查询
                    var jobJxdUnionPayQuery = JobBuilder.Create<JxdUnionPayQueryJob>()
                       .WithIdentity($"jxdunionpayquery{system.Name}", "jxdunionpayquery")
                       .Build();
                    jobJxdUnionPayQuery.JobDataMap.Put(JobParaName.ParaSystemName, system.Name);
                    jobJxdUnionPayQuery.JobDataMap.Put(JobParaName.ParaConnStrName, system.ConnStr);

                    var triggerjxdunionpayquery = TriggerBuilder.Create()
                        .WithIdentity($"jxdunionpayquery{system.Name}", "jxdunionpayquery")
                        .WithSimpleSchedule(s => s.WithIntervalInSeconds(businessOption.QueryInterval).RepeatForever())
                        .Build();
                    await _scheduler.ScheduleJob(jobJxdUnionPayQuery, triggerjxdunionpayquery);
                    //启动退款任务
                    var refundJob = JobBuilder.Create<WaitRefundJob>()
                        .WithIdentity($"WaitRefundJob{system.Name}", "WaitRefundJob")
                        .Build();
                    refundJob.JobDataMap.Put(JobParaName.ParaSystemName, system.Name);
                    refundJob.JobDataMap.Put(JobParaName.ParaConnStrName, system.ConnStr);

                    var triggerRefundJob = TriggerBuilder.Create()
                        .WithIdentity($"WaitRefundJob{system.Name}", "WaitRefundJob")
                        .WithSimpleSchedule(s => s.WithIntervalInSeconds(businessOption.QueryInterval).RepeatForever())
                        .Build();

                    await _scheduler.ScheduleJob(refundJob, triggerRefundJob);


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
