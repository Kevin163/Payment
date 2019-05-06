namespace GemstarPaymentCore.Business.BusinessHandlers
{
    /// <summary>
    /// 业务处理的其他参数
    /// </summary>
    public class BusinessHandlerParameter
    {
        /// <summary>
        /// 支付成功后回调通知本地支付状态的本地接口回调地址
        /// </summary>
        public string CallbackUrl { get; set; }
        /// <summary>
        /// 本地会员接口地址，用于调用会员接口
        /// </summary>
        public string MemberUrl { get; set; }
        /// <summary>
        /// 是否由本地接口程序转发来的请求，有些业务需要根据此值进行不同的处理
        /// </summary>
        public bool IsFromRedirect { get; set; }
    }
}
