using System;
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
        /// 扫呗支付聚合支付的二维码链接地址
        /// </summary>
        public string LcswPayUnionQrcodeUrl { get; set; }
    }
}
