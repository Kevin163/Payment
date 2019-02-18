using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Gemstar.Extensions.Logging.AliyunLogService;

namespace GemstarPaymentCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
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
    }
}
