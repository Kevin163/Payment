using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers
{
    public class HttpSwitchHandler : IBusinessHandler
    {
        private ILogger _log;
        private IHttpClientFactory _clientFactory;
        private string _businessContent;
        public HttpSwitchHandler(ILogger<HttpSwitchHandler> log,IHttpClientFactory clientFactory)
        {
            _log = log;
            _clientFactory = clientFactory;
        }


        public void SetBusinessContent(string businessContent)
        {
            _businessContent = businessContent;
        }
        public async Task<HandleResult> HandleBusinessContentAsync()
        {
            var paras = _businessContent.Split('|');
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
                    using (var requestContent = new StringContent(body, Encoding.UTF8, contentType))
                    using (var response = await client.PostAsync(url, requestContent))
                    using (var responseContent = response.Content)
                    {
                        var result = await responseContent.ReadAsStringAsync();
                        return HandleResult.Success(result);
                    }
                }                
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
