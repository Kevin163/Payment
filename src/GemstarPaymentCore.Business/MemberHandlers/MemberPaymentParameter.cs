namespace GemstarPaymentCore.Business.MemberHandlers
{
    /// <summary>
    /// 会员支付参数
    /// </summary>
    public class MemberPaymentParameter
    {
        /// <summary>
        /// 会员id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 营业点代码
        /// </summary>
        public string OutletCode { get; set; }
        /// <summary>
        /// 要支付的金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 原始单号
        /// </summary>
        public string OrigBillNo { get; set; }
        /// <summary>
        /// 相关号码
        /// </summary>
        public string RefNo { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 会员扣款密码
        /// </summary>
        public string Password { get; set; }
    }
}
