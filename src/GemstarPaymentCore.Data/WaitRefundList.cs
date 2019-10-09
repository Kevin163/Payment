using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GemstarPaymentCore.Data
{
    /// <summary>
    /// 待退款记录
    /// </summary>
    [Table("WaitRefundList")]
    public class WaitRefundList
    {
        /// <summary>
        /// 退款id，主键值
        /// </summary>
        [Key]
        public string RefundId { get; set; }

        /// <summary>
        /// 支付类型，将决定了如何进行退款，支持的类型:weixinpay,alipay,lcswpay
        /// </summary>
        public string PayType { get; set; }
        /// <summary>
        /// 退款参数，不同的支付类型需要不同的参数，以json格式进行传递，具体的参数格式见文档https://www.yuque.com/gemstar/works/payment_refund
        /// </summary>
        public string RefundPara { get; set; }
        /// <summary>
        /// 退款状态,notSend:未发送退款请求，sended：已发送退款请求，success：退款成功，fail：退款失败，其他中间状态字符串
        /// </summary>
        public string RefundStatus { get; set; }
        /// <summary>
        /// 退款失败原因
        /// </summary>
        public string RefundFailReason { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 发送退款请求日期
        /// </summary>
        public DateTime? SendDate { get; set; }

        public class RefundPayType
        {
            /// <summary>
            /// 微信支付
            /// </summary>
            public const string PayTypeWeixin = "weixinpay";
            /// <summary>
            /// 支付宝支付
            /// </summary>
            public const string PayTypeAlipay = "alipay";
            /// <summary>
            /// 扫呗支付
            /// </summary>
            public const string PayTypeLcsw = "lcswpay";
        }
        public class RefundStatu
        {
            /// <summary>
            /// 未发送退款请求
            /// </summary>
            public const string StatuNotSend = "notSend";
            /// <summary>
            /// 已发送退款请求
            /// </summary>
            public const string StatuSended = "sended";
            /// <summary>
            /// 退款成功
            /// </summary>
            public const string StatuSuccess = "success";
            /// <summary>
            /// 退款失败
            /// </summary>
            public const string StatuFail = "fail";
        }
            
    }
}
