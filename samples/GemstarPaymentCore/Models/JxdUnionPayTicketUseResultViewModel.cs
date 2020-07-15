namespace GemstarPaymentCore.Models
{
    /// <summary>
    /// 聚合支付时，使用优惠券后的结果
    /// </summary>
    public class JxdUnionPayTicketUseResultViewModel
    {
        /// <summary>
        /// 优惠券使用是否成功
        /// </summary>
        public bool TicketUseSuccess { get; set; }
        /// <summary>
        /// 优惠券抵扣金额
        /// </summary>
        public decimal TicketAmount { get; set; }
        /// <summary>
        /// 提示信息
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 是否需要继续支付
        /// </summary>
        public bool NeedPay { get; set; }
        /// <summary>
        /// 需要继续支付的金额
        /// </summary>
        public decimal NeedPayAmount { get; set; }
        /// <summary>
        /// 需要继续支付的支付地址
        /// </summary>
        public string NeedPayUrl { get; set; }
    }
}
