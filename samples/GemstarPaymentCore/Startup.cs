using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Essensoft.AspNetCore.Payment.Alipay;
using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.WeChatPay;
using GemstarPaymentCore.Business;
using GemstarPaymentCore.Business.BusinessHandlers;
using GemstarPaymentCore.Business.MemberHandlers;
using GemstarPaymentCore.Data;
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
            services.AddOptions();
            //引入业务参数
            services.Configure<BusinessOption>(Configuration.GetSection("Business"));
            //引入利楚商务扫呗支付
            services.Configure<LcswPayOption>(Configuration.GetSection("LcswPay"));
            services.AddLcswPay();
            //引入微信服务商证书
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

            services.AddWeChatPay(opt=> { opt.AppId = ConfigHelper.WxProviderAppId;opt.MchId = ConfigHelper.WxProviderMchId; opt.Key = ConfigHelper.WxProviderKey; opt.CreateIP = localIp;opt.NotifyUrl = notifyUrl; });
            //引入支付宝
            services.AddAlipay();
            services.Configure<AlipayOptions>(Configuration.GetSection("Alipay"));
            //引入业务处理类
            services.AddBusinessHandlers();
            services.AddScoped<IWxPayDBFactory, WxPayDBFactory>();
            services.AddScoped<IMemberHandlerFactory, MemberHandlerFactory>();

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
