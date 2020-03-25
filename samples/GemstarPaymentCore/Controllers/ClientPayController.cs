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
using System.Linq;
using GemstarPaymentCore.Business.Utility;
using System.Data.SqlClient;

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
        public async Task<IActionResult> Index(string payStr,string callbackUrl,string memberUrl,string memberType,string memberBindUrl,string bspmsGrpid,string bspmsChannelCode,string bspmsChannelKey,int? redirect=0,int? isFromRedirect = 0,int? isEncrypted = 1)
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
                        //如果当前账号是加密的，则先执行解密
                        if(isEncrypted == 1)
                        {
                            payStr = Decrypt(payStr);
                        }
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
                                ,KeyValuePair.Create("bspmsGrpid",businessOption.BsPmsGrpId)
                                ,KeyValuePair.Create("bspmsChannelCode",businessOption.BsPmsChannelCode)
                                ,KeyValuePair.Create("bspmsChannelKey",businessOption.BsPmsChannelKey)
                                ,KeyValuePair.Create("isFromRedirect","1")
                                ,KeyValuePair.Create("isEncrypted","0")}
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
                            IsFromRedirect = isFromRedirect == 1,
                            BspmsGrpid = bspmsGrpid,
                            BspmsChannelCode = bspmsChannelCode,
                            BspmsChannelKey = bspmsChannelKey
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

        private static string UserSeriesNo = "";
        private string Decrypt(string payStr)
        {
            try
            {
                //取出解密密钥，密钥为连接的数据库中的cyuserinfo表中的用户序列号
                //如果没有设置任何数据库连接则不能获取到有效加密密钥，则直接认为不需要解密，直接返回原文
                //线上的payment数据库中的用户序列号设置为固定值PaymentNotEncrypted，如果是这个固定值，则表示是线上系统调用或者是线下系统转发过来的支付请求，此时不需要解密。因为线上系统的账号用户不能直接修改，直接就是明文保存的。线下转发的已经解密完成了，不需要解密
                if (string.IsNullOrEmpty(UserSeriesNo))
                {
                    var businessOption = _serviceProvider.GetService<IOptions<BusinessOption>>().Value;
                    var businessSystemInfo = businessOption.Systems.FirstOrDefault(w => w.HavePay == 1);
                    if (businessSystemInfo == null)
                    {
                        return payStr;
                    }
                    var connStr = businessSystemInfo.ConnStr;
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "select v_SeriesNo from cyUserInfo";
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserSeriesNo = reader.GetString(0);
                            }
                            reader.Close();
                        }
                        conn.Close();
                    }
                }
                if(string.IsNullOrEmpty(UserSeriesNo) || "PaymentNotEncrypted".Equals(UserSeriesNo))
                {
                    return payStr;
                }
                //调用各业务组件进行解密，只需要解密收款账号，由于不同的业务请求收款账号位置不同，不能统一处理
                var para = new BusinessHandlerParameter();
                var handler = BusinessHandlerFactory.GetHandler(payStr, para, _serviceProvider);
                var security = _serviceProvider.GetService<ISecurity>();
                return handler.Decrypt(payStr, UserSeriesNo, security);
            } catch(Exception ex)
            {
                //解密有异常时，直接返回原始字符串
                _logger.LogError(_eventId, ex, $"解密字符串{payStr}时遇到异常{ex.Message}");
                return payStr;
            }
        }
    }
}