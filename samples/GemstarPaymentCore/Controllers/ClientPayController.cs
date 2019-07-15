using System;
using System.Net.Http;
using System.Threading.Tasks;
using GemstarPaymentCore.Business;
using GemstarPaymentCore.Business.BusinessHandlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

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
        public async Task<IActionResult> Index(string payStr,string callbackUrl,string memberUrl,string memberType,string memberBindUrl,int? redirect=0,int? isFromRedirect = 0)
        {
            using (var scope = _logger.BeginScope(this))
            {
                //增加一个请求id,用于区分日志记录中的接收到的内容和返回的内容是否同一请求
                var requestId = Guid.NewGuid().ToString();
                try
                {
                    if (!string.IsNullOrWhiteSpace(payStr))
                    {
                        _logger.LogInformation(_eventId, $"请求{requestId}于{DateTime.Now.ToString("mm:ss.fff")}接收到的业务字符串：{payStr}");

                        if (redirect == 1)
                        {
                            //将求转发给线上地址进行处理
                            var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
                            var httpClient = httpClientFactory.CreateClient();
                            var businessOption = _serviceProvider.GetService<IOptionsSnapshot<BusinessOption>>().Value;
                            if (string.IsNullOrWhiteSpace(businessOption.JxdPaymentUrl))
                            {
                                var error = HandleResult.Fail("指定redirect=1时，必须先设置JxdPaymentUrl地址");
                                 _logger.LogInformation(_eventId, $"请求{requestId}于{DateTime.Now.ToString("mm:ss.fff")}返回的业务字符串：{error.Content}");
                                return Content(error.ResultStr);
                            }
                            using (var requestContent = new FormUrlEncodedContent(
                                new List<KeyValuePair<string, string>>{
                                KeyValuePair.Create("payStr",payStr)
                                ,KeyValuePair.Create("callbackUrl",businessOption.InternetUrl)
                                ,KeyValuePair.Create("memberUrl",businessOption.InternetMemberUrl)
                                ,KeyValuePair.Create("memberType",businessOption.MemberVersion)
                                ,KeyValuePair.Create("memberBindUrl",businessOption.MemberBindWxUrl)
                                ,KeyValuePair.Create("isFromRedirect","1") }
                            ))
                            using (var response = await httpClient.PostAsync(businessOption.JxdPaymentUrl, requestContent))
                            using (var responseContent = response.Content)
                            {
                                var resultStr = await responseContent.ReadAsStringAsync();
                                 _logger.LogInformation(_eventId, $"请求{requestId}于{DateTime.Now.ToString("mm:ss.fff")}返回的业务字符串：{resultStr}");
                                return Content(resultStr);
                            }
                        }
                        var para = new BusinessHandlerParameter
                        {
                            CallbackUrl = callbackUrl,
                            MemberUrl = memberUrl,
                            MemberType = memberType,
                            MemberBindUrl = memberBindUrl,
                            IsFromRedirect = isFromRedirect == 1
                        };
                        var handler = BusinessHandlerFactory.GetHandler(payStr,para, _serviceProvider);
                        var result = await handler.HandleBusinessContentAsync();
                        _logger.LogInformation(_eventId, $"请求{requestId}于{DateTime.Now.ToString("mm:ss.fff")}返回的业务字符串：{result.ResultStr}");
                        return Content(result.ResultStr);
                    }
                    else
                    {
                        var result = HandleResult.Fail("请指定payStr参数内容。看到此信息则表示接口地址已经正确，可以直接复制浏览器地址栏中的地址（注意，不能使用localhost/127.0.0.1之类的本地地址）填写到业务系统中");
                        return Content(result.ResultStr);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(_eventId, ex, $"请求{requestId}于{DateTime.Now.ToString("mm:ss.fff")}执行业务时遇到异常:{ex.Message}");
                    var result = HandleResult.Fail(ex);
                    return Content(result.ResultStr);
                }
            }
        }
    }
}