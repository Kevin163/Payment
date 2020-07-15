using System;
using System.Collections.Generic;
using System.Text;

namespace GemstarPaymentCore.Business.MemberHandlers
{
    /// <summary>
    /// 会员券信息
    /// </summary>
    public class MemberTicketInfo
    {
        /// <summary>
        /// 会员id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 券编码
        /// </summary>
        public string TicketCode { get; set; }
        /// <summary>
        /// 券类型编码
        /// </summary>
        public string TicketTypeCode { get; set; }
        /// <summary>
        /// 券类型名称
        /// </summary>
        public string TicketTypeCname { get; set; }
        /// <summary>
        /// 是否已经使用
        /// </summary>
        public bool IsUsed { get; set; }
        /// <summary>
        /// 券金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 有效期，过期时间
        /// </summary>
        public DateTime ExpiredDate { get; set; }
    }
}
