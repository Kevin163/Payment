using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GemstarPaymentCore.Data
{
    /// <summary>
    /// 捷信达聚合支付_扫呗支付记录表
    /// </summary>
    [Table("unionPay_lcsw")]
    public class UnionPayLcsw
    {
        /// <summary>
        /// 主键值
        /// </summary>
        [Key]
        public Guid Id { get; set; }
        /// <summary>
        /// 业务系统
        /// 用于记录是哪个业务系统的单据，以便回调通知时知道修改哪个业务系统中的数据
        /// </summary>
        public string SystemName { get; set; }
        /// <summary>
        /// 终端流水号，填写商户系统的订单号
        /// </summary>
        public string TerminalTrace { get; set; }
        /// <summary>
        /// 终端交易时间，yyyyMMddHHmmss
        /// </summary>
        public DateTime TerminalTime { get; set; }
        /// <summary>
        /// 扫呗商户号
        /// </summary>
        public string MerchantNo { get; set; }
        /// <summary>
        /// 扫呗终端号
        /// </summary>
        public string TerminalId { get; set; }
        /// <summary>
        /// 终端访问令牌
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// 营业点代码
        /// </summary>
        public string OutletCode { get; set; }
        /// <summary>
        /// 金额，单位元
        /// </summary>
        public decimal TotalFee { get; set; }
        /// <summary>
        /// 订单描述
        /// </summary>
        public string OrderBody { get; set; }
        /// <summary>
        /// 附加数据，原样返回
        /// </summary>
        public string Attach { get; set; }
        /// <summary>
        /// 支付回调地址
        /// </summary>
        public string CallbackUrl { get; set; }
        /// <summary>
        /// 会员接口地址
        /// </summary>
        public string MemberUrl { get; set; }
        /// <summary>
        /// 会员类型
        /// </summary>
        public string MemberType { get; set; }
        /// <summary>
        /// 会员绑定地址
        /// </summary>
        public string MemberBindUrl { get; set; }
        /// <summary>
        /// 会员参数
        /// </summary>
        public string MemberPara { get; set; }
        /// <summary>
        /// 扫呗支付聚合支付的二维码链接地址
        /// </summary>
        public string LcswPayUnionQrcodeUrl { get; set; }
        /// <summary>
        /// 公众号id
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 公众号密钥
        /// </summary>
        public string AppSecret { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public WxPayInfoStatus Status { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? Paytime { get; set; }
        /// <summary>
        /// 支付流水号
        /// </summary>
        public string PayTransId { get; set; }
        /// <summary>
        /// 支付说明
        /// </summary>
        public string PayRemark { get; set; }
        /// <summary>
        /// 支付方式，Member:会员卡支付,010：微信，020：支付宝，060：qq钱包，080：京东钱包，090：口碑，100：翼支付
        /// </summary>
        public string PayType {get;set;}
    }
}
