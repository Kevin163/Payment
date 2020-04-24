using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GemstarPaymentCore.Data
{
    /// <summary>
    /// 捷信达聚合支付_扫呗支付记录明细表
    /// </summary>
    [Table("unionPay_lcsw_detail")]
    public class UnionPayLcswDetail
    {
        /// <summary>
        /// 会员优惠券支付方式
        /// </summary>
        public const string PayTypeMemberTicket = "MemberTicket";

        /// <summary>
        /// 扫呗利楚商务支付方式
        /// </summary>
        public const string PayTypeLcsw = "lcsw";

        /// <summary>
        /// 会员储值支付方式
        /// </summary>
        public const string PayTypeMember = "Member";
        /// <summary>
        /// 根据主键生成的商户订单号
        /// </summary>
        public string TerminalTrace => DetailId.ToString("N");
        /// <summary>
        /// 主键值
        /// </summary>
        [Key]
        public Guid DetailId { get; set; }
        /// <summary>
        /// 支付记录id
        /// </summary>
        public Guid PayId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CDate { get; set; }
        /// <summary>
        /// 应付金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 支付状态
        /// </summary>
        public WxPayInfoStatus PayStatus { get; set; }
        /// <summary>
        /// 二维码支付地址
        /// </summary>
        public string PayQrcodeUrl { get; set; }
        /// <summary>
        /// 支付方式：ticket:会员优惠券支付，lcsw:扫呗聚合支付
        /// </summary>
        public string PayType { get; set; }
        /// <summary>
        /// 实际支付金额
        /// </summary>
        public decimal? PaidAmount { get; set; }
        /// <summary>
        /// 实际支付时间
        /// </summary>
        public DateTime? PaidTime { get; set; }
        /// <summary>
        /// 实际支付流水号，当为会员优惠券支付时，此时保存的是优惠券的券号
        /// </summary>
        public string PaidTransNo { get; set; }
    }
}
