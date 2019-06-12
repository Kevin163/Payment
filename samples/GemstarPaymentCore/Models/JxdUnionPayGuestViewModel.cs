namespace GemstarPaymentCore.Models
{
    /// <summary>
    /// 捷信达聚合支付的非会员支付视图模型
    /// </summary>
    public class JxdUnionPayGuestViewModel
    {
        /// <summary>
        /// 支付说明
        /// </summary>
        public string OrderTitle { get; set; }
        /// <summary>
        /// 支付记录id
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// 待支付费用
        /// </summary>
        public decimal PayFee { get; set; }
        /// <summary>
        /// 微信支付链接地址
        /// </summary>
        public string WxPayUrl { get; set; }
        /// <summary>
        /// 微信会员绑定地址
        /// </summary>
        public string MemberBindUrl { get; set; }
    }
}
