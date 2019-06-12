using GemstarPaymentCore.Business.MemberHandlers;
using System.Collections.Generic;

namespace GemstarPaymentCore.Models
{
    /// <summary>
    /// 捷信达聚合支付的会员支付视图模型
    /// </summary>
    public class JxdUnionPayMemberViewModel
    {
        /// <summary>
        /// 支付说明
        /// </summary>
        public string OrderTitle { get; set; }
        /// <summary>
        /// 会员id
        /// </summary>
        public string MemberId { get; set; }
        /// <summary>
        /// 支付记录id
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// 待支付费用
        /// </summary>
        public decimal PayFee { get; set; }
        /// <summary>
        /// 会员卡支付密码
        /// </summary>
        public string CardPassword { get; set; }
        /// <summary>
        /// 微信支付链接地址
        /// </summary>
        public string WxPayUrl { get; set; }
        /// <summary>
        /// 会员卡余额
        /// </summary>
        public decimal MemberBalance { get; set; }
        /// <summary>
        /// 会员的所有卡信息
        /// </summary>
        public List<MemberInfo> MemberInfos { get; set; }

    }
}
