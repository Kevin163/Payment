using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.Utility
{
    /// <summary>
    /// HttpClient扩展类
    /// </summary>
    public static class HttpClientExtension
    {
        /// <summary>
        /// 执行HTTP POST请求。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url">请求地址</param>
        /// <param name="content">请求内容</param>
        /// <returns>HTTP响应</returns>
        public static async Task<string> DoPostAsync(this HttpClient client, string url, string content,string contentType = "application/x-www-form-urlencoded")
        {
            using (var requestContent = new StringContent(content, Encoding.UTF8, contentType))
            using (var response = await client.PostAsync(url, requestContent))
            using (var responseContent = response.Content)
            {
                return await responseContent.ReadAsStringAsync();
            }
        }
    }
}
