using System;
using GemstarPaymentCore.Payment.ChinaumsPay.Response;

namespace GemstarPaymentCore.Payment.ChinaumsPay.Request
{
    /// <summary>
    /// 银商支付和退款查询请求
    /// </summary>
    public class ChinaumsPayQueryRequest : IChinaumsPayRequest<ChinaumsPayQueryResponse>
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string MsgId { get; set; }
        /// <summary>
        /// 消息来源
        /// </summary>
        public string MsgSrc { get; set; }
        /// <summary>
        /// 账单号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 退款订单号
        /// </summary>
        public string RefundOrderId { get; set; }
        /// <summary>
        /// 订单时间，格式yyyy-MM-dd
        /// </summary>
        public string BillDate { get; set; }
        public ChinaumsPayDictionary GetParameters()
        {
            return new ChinaumsPayDictionary
            {
                {"msgId",MsgId },
                {"msgSrc",MsgSrc },
                {"msgType","bills.query" },
                {"requestTimestamp",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },//报文请求时间，格式yyyy-MM-dd HH:mm:ss 
                {"billNo",BillNo },
                {"refundOrderId",RefundOrderId },
                {"billDate",BillDate },

            };
        }
    }
}
