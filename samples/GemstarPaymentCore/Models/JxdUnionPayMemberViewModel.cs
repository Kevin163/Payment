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
        public List<MemberInfoDisplay> MemberInfos { get; set; }

    }
    /// <summary>
    /// 会员显示信息
    /// </summary>
    public class MemberInfoDisplay
    {
        public MemberInfoDisplay(MemberInfo info)
        {
            Id = info.Id;
            CardTypeName = info.CardTypeName;
            CardNo = info.CardNo;
            if (CardNo?.Length > 4)
            {
                CardNoDisplay = $"****{CardNo.Substring(CardNo.Length - 4, 4)}";
            }
            else
            {
                CardNoDisplay = CardNo;
            }
            Balance = info.Balance;
            HavePassword = info.HavePassword ? "1" : "0";
        }
        /// <summary>
        /// 会员主键
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 会员卡类型名称
        /// </summary>
        public string CardTypeName { get; set; }
        /// <summary>
        /// 会员卡号
        /// </summary>
        public string CardNo { get; set; }
        /// <summary>
        /// 会员卡号显示
        /// </summary>
        public string CardNoDisplay { get; set; }
        /// <summary>
        /// 会员可用储值余额
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 是否拥有密码
        /// </summary>
        public string HavePassword { get; set; }

    }
}
