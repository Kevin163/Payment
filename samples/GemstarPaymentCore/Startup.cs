using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.WeChatPay;
using GemstarPaymentCore.Business;
using GemstarPaymentCore.Business.BusinessHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GemstarPaymentCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.Configure<LcswPayOption>(Configuration.GetSection("LcswPay"));
            services.AddLcswPay();

            services.AddHttpClient();
            services.AddHttpClient(ConfigHelper.WxPayCertificateName).ConfigurePrimaryHttpMessageHandler(() =>
            {
                // 载入证书
                var clientAssembly = Assembly.GetAssembly(this.GetType());
                var stream = clientAssembly.GetManifestResourceStream("GemstarPaymentCore.Models.apiclient_cert.p12");
                var certData = new byte[stream.Length];
                stream.Read(certData, 0, certData.Length);
                var streamCert = new X509Certificate2(certData, "1345752201");
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(streamCert);
                return handler;
            });
            var localIp = "";
            var localName = Dns.GetHostName();
            var ipAddress = Dns.GetHostAddresses(localName);
            foreach(var ip in ipAddress)
            {
                if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIp = ip.ToString();
                    break;
                }
            }
            var notifyUrl = Configuration.GetValue<string>("WechatPay:NotifyUrl");

            services.AddWeChatPay(opt=> { opt.AppId = "wx4cea1ae3f21c72e8";opt.MchId = "1345752201"; opt.Key = "ShenZhenJieXinDa4007755123364567"; opt.CreateIP = localIp;opt.NotifyUrl = notifyUrl; });
            services.AddBusinessHandlers();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            } else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
