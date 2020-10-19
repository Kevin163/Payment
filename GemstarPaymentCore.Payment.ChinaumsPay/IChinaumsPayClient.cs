using System.Threading.Tasks;

namespace GemstarPaymentCore.Payment.ChinaumsPay
{
    /// <summary>
    /// 银商支付客户端实例
    /// </summary>
    public interface IChinaumsPayClient
    {
        /// <summary>
        /// 执行请求，并且返回对应的响应
        /// </summary>
        /// <typeparam name="T">响应类型</typeparam>
        /// <param name="request">请求实例</param>
        /// <returns>请求执行后的响应</returns>
        Task<T> ExecuteRequestAsync<T>(IChinaumsPayRequest<T> request, ChinaumsPayOption option) where T : ChinaumsPayResponse;
    }
}
