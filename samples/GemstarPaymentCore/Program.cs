using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gemstar.Extensions.Logging.AliyunLogService;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

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
                args.Where(arg => arg != "--console").ToArray(), isService);

            var host = builder.Build();

            if (isService)
            {
                // To run the app without the CustomWebHostService change the
                // next line to host.RunAsService();
                host.RunAsService();
            } else
            {
                var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
                try
                {
                    logger.Debug("init main");
                    host.Run();
                } catch (Exception ex)
                {
                    logger.Error(ex, "Stopped program because of exception");
                    throw;
                } finally
                {
                    NLog.LogManager.Shutdown();
                }
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
            if (string.IsNullOrEmpty(portStr) || !int.TryParse(portStr, out port))
            {
                port = 8999;
            }
            var hostBuilder = WebHost.CreateDefaultBuilder(args)
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders();
                //有些电脑上，向console输出日志的时候，会报存储空间不足，无法处理此命令的错误，所以此处清除后，不再添加任何日志记录，只由最外层的nlog来记录
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
    }
}
