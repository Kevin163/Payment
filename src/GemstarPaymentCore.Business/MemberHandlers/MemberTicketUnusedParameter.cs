namespace GemstarPaymentCore.Business.MemberHandlers
{
    /// <summary>
    /// 会员券反核销参数
    /// </summary>
    public class MemberTicketUnusedParameter
    {
        /// <summary>
        /// 会员id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 要反核销的券id
        /// </summary>
        public string TicketCode { get; set; }
        /// <summary>
        /// 反核销说明
        /// </summary>
        public string Remark { get; set; }
    }
}
