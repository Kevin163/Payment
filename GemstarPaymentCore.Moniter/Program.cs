using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GemstarPaymentCore.Moniter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var host = new HostBuilder()
                    .ConfigureHostConfiguration(configSettings)
                    .ConfigureLogging((context, logging) => { logging.AddConsole(); })
                    .ConfigureServices(configServices)
                    .Build();
                host.Run();
            } catch
            {
                //当ctrl+c退出程序后，host.run会报一个异常，此处忽略此异常
            }
        }

        private static void configSettings(IConfigurationBuilder config)
        {
            config.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("monitersettings.json");
        }

        private static void configServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ProcessOption>(context.Configuration.GetSection("ProcessItems"));
            services.AddHostedService<ProcessHostedService>();
        }
    }
}
