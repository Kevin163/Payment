using GemstarPaymentCore.Payment.ChinaumsPay;

namespace GemstarPaymentCore.Business.BusinessHandlers.ChinaumsPay
{
    /// <summary>
    /// 银商支付响应扩展类
    /// </summary>
    public static class ChinaumsPayResponseExtension
    {
        /// <summary>
        /// 判断响应是否正确
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool IsSuccessCode(this ChinaumsPayResponse response)
        {
            return response.ErrCode == "SUCCESS";
        }
        /// <summary>
        /// 获取响应的错误原因
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static HandleResult FailResult(this ChinaumsPayResponse response)
        {
            return HandleResult.Fail($"{response.ErrMsg}");
        }
    }
}
