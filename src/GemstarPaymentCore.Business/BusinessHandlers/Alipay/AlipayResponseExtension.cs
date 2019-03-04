using Essensoft.AspNetCore.Payment.Alipay;

namespace GemstarPaymentCore.Business.BusinessHandlers.Alipay
{
    /// <summary>
    /// 支付宝支付响应扩展类
    /// </summary>
    public static class AlipayResponseExtension
    {
        /// <summary>
        /// 判断响应是否是成功响应
        /// </summary>
        /// <param name="response">响应实例</param>
        /// <returns>是否是成功响应</returns>
        public static bool IsSuccessCode(this AlipayResponse response)
        {
            return response != null && response.Code == "10000";
        }
        /// <summary>
        /// 返回响应的错误信息结果
        /// </summary>
        /// <param name="response">响应实例</param>
        /// <returns>错误信息结果</returns>
        public static HandleResult FailResult(this AlipayResponse response)
        {
            if (response == null)
            {
                return HandleResult.Fail("未知错误，可能是网络不通或者密钥不正确导致无法解析结果");
            }
            return HandleResult.Fail(response.SubMsg);
        }
    }
}
