using System.Net.Http;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Models
{
    public class PaymentCallback
    {
        public static async Task CallbackNotify(PaymentCallbackParameter para, IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient();
            using (var requestContent = new FormUrlEncodedContent(para.ToKeyValuePairs))
            using (var response = await httpClient.PostAsync(para.CallbackUrl, requestContent))
            {
            }
        }
    }
}
