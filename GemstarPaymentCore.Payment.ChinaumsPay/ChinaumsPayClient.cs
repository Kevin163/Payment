using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GemstarPaymentCore.Payment.ChinaumsPay.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GemstarPaymentCore.Payment.ChinaumsPay
{
    /// <summary>
    /// 银商支付客户端默认实现
    /// </summary>
    public class ChinaumsPayClient : IChinaumsPayClient
    {
        private IHttpClientFactory _httpClientFactory;
        private ILogger _logger;
        public ChinaumsPayClient(ILogger<ChinaumsPayClient> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 执行请求，并且返回对应的响应
        /// </summary>
        /// <typeparam name="T">响应类型</typeparam>
        /// <param name="request">请求实例</param>
        /// <returns>请求执行后的响应</returns>
        public async Task<T> ExecuteRequestAsync<T>(IChinaumsPayRequest<T> request,ChinaumsPayOption option) where T : ChinaumsPayResponse
        {
            var parameters = request.GetParameters();
            parameters.Add("mid", option.MId);
            parameters.Add("tid", option.TId);
            //parameters.Add("signType", "SHA256");
            var sign = ChinaumsPaySignature.SignWithKey(parameters, option.SignKey);
            parameters.Add("sign", sign);

            var requestBody = JsonConvert.SerializeObject(parameters);
            _logger.LogInformation($"开始请求银商支付接口，地址：{option.ApiUrl},内容：{requestBody}");
            using (var content = new StringContent(requestBody, Encoding.UTF8, "application/json"))
            using (var client = _httpClientFactory.CreateClient())
            using (var response = await client.PostAsync(option.ApiUrl, content))
            {
                var resultStr = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"接收到银商支付接口返回，内容：{resultStr}");
                return JsonConvert.DeserializeObject<T>(resultStr);
            }

        }
    }
}
