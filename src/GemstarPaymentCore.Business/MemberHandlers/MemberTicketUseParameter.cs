namespace GemstarPaymentCore.Business.MemberHandlers
{
    /// <summary>
    /// 会员优惠券使用参数
    /// </summary>
    public class MemberTicketUseParameter
    {
        /// <summary>
        /// 会员id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 要使用的券编号
        /// </summary>
        public string TicketCode { get; set; }
        /// <summary>
        /// 当前操作员
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 营业点代码
        /// </summary>
        public string OutletCode { get; set; }
        /// <summary>
        /// 相关号码
        /// </summary>
        public string RefNo { get; set; }
    }
}
