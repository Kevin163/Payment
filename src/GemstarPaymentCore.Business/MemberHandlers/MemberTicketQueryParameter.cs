namespace GemstarPaymentCore.Business.MemberHandlers
{
    /// <summary>
    /// 查询会员券明细的参数
    /// </summary>
    public class MemberTicketQueryParameter
    {
        /// <summary>
        /// 会员id，如果是有会员绑定了多张卡之类的，则此id应该就是以逗号分隔的会员id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 营业点代码
        /// </summary>
        public string OutletCode { get; set; }
    }
}
