using System;
using System.Net.Http;
using System.Threading.Tasks;
using GemstarPaymentCore.Business;
using GemstarPaymentCore.Business.BusinessHandlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.Extensions.Options;

namespace GemstarPaymentCore.Controllers
{
    public class ClientPayController : Controller
    {
        private ILogger<ClientPayController> _logger;
        private IServiceProvider _serviceProvider;
        private EventId _eventId = new EventId(1,nameof(ClientPayController));
        public ClientPayController(ILogger<ClientPayController> logger,IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public async Task<IActionResult> Index(string payStr,int? redirect=0)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(payStr))
                {
                    _logger.LogInformation(_eventId,$"接收到的业务字符串：{payStr}");

                    if(redirect == 1)
                    {
                        //将求转发给线上地址进行处理
                        var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
                        var httpClient = httpClientFactory.CreateClient();
                        var businessOption = _serviceProvider.GetService<IOptions<BusinessOption>>().Value;
                        if (string.IsNullOrWhiteSpace(businessOption.JxdPaymentUrl))
                        {
                            var error = HandleResult.Fail("指定redirect=1时，必须先设置JxdPaymentUrl地址");
                            return Content(error.ResultStr);
                        }
                        using (var requestContent = new StringContent($"payStr={payStr}", Encoding.UTF8, "application/x-www-form-urlencoded"))
                        using (var response = await httpClient.PostAsync(businessOption.JxdPaymentUrl, requestContent))
                        using (var responseContent = response.Content)
                        {
                            var resultStr = await responseContent.ReadAsStringAsync();
                            return Content(resultStr);
                        }
                    }
                    var handler = BusinessHandlerFactory.GetHandler(payStr,_serviceProvider);
                    var result = await handler.HandleBusinessContentAsync();
                    _logger.LogInformation(_eventId,$"返回的业务字符串：{result.ResultStr}");
                    return Content(result.ResultStr);
                } else
                {
                    var result = HandleResult.Fail("请指定payStr参数内容。看到此信息则表示接口地址已经正确，可以直接复制浏览器地址栏中的地址（注意，不能使用localhost/127.0.0.1之类的本地地址）填写到业务系统中");
                    return Content(result.ResultStr);
                }
            } catch (Exception ex)
            {
                _logger.LogError(_eventId, ex, "执行业务时遇到异常");
                var result = HandleResult.Fail(ex);
                return Content(result.ResultStr);
            }
        }
    }
}