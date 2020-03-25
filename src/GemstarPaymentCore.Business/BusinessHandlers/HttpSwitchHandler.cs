using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GemstarPaymentCore.Business.Utility;

namespace GemstarPaymentCore.Business.BusinessHandlers
{
    public class HttpSwitchHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private IHttpClientFactory _clientFactory;
        public HttpSwitchHandler(ILogger<HttpSwitchHandler> log,IHttpClientFactory clientFactory)
        {
            _log = log;
            _clientFactory = clientFactory;
        }
        protected override string contentFormat => "url";
        protected override int[] contentEncryptedIndexs => new int[] { };
        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] paras)
        {
            if (paras.Length < 3)
            {
                return await DoGet(paras[0]);
            } else
            {
                return await DoPost(paras[0], paras[1], paras[2]);
            }
        }
        private async Task<HandleResult> DoGet(string url)
        {
            try
            {
                using (var client = _clientFactory.CreateClient())
                {
                    var result = await client.GetStringAsync(url);
                    return HandleResult.Success(result);
                }
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
        private async Task<HandleResult> DoPost(string url, string contentType, string body)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(body ?? "");
                if (string.IsNullOrWhiteSpace(contentType))
                {
                    contentType = "application/x-www-form-urlencoded";
                }

                using (var client = _clientFactory.CreateClient())
                {
                    var result = await client.DoPostAsync(url, body, contentType);
                    return HandleResult.Success(result);
                }
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
