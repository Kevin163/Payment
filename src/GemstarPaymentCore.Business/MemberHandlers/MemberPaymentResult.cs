namespace GemstarPaymentCore.Business.MemberHandlers
{
    /// <summary>
    /// 会员支付结果
    /// </summary>
    public class MemberPaymentResult
    {
        /// <summary>
        /// 是否支付成功
        /// </summary>
        public bool PaySuccess { get; set; }
        /// <summary>
        /// 提示消息，支付不成功时为支付不成功的原因
        /// </summary>
        public string Message { get; set; }
    }
}
