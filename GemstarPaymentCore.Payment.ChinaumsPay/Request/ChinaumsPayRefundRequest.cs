using System;
using GemstarPaymentCore.Payment.ChinaumsPay.Response;

namespace GemstarPaymentCore.Payment.ChinaumsPay.Request
{
    /// <summary>
    /// 银商支付退款申请
    /// </summary>
    public class ChinaumsPayRefundRequest : IChinaumsPayRequest<ChinaumsPayRefundResponse>
    {
        /// <summary>
        /// 消息ID，原样返回
        /// </summary>
        public string MsgId { get; set; }
        /// <summary>
        /// 消息来源
        /// </summary>
        public string MsgSrc { get; set; }
        /// <summary>
        /// 请求系统预留字段
        /// </summary>
        public string SrcReserve { get; set; }
        /// <summary>
        /// 账单时间，格式yyyy-MM-dd
        /// </summary>
        public string BillDate { get; set; }
        /// <summary>
        /// 商户账单号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 退货交易的订单号，如不指定，则系统自动生成。
        /// 如商户指定，须以4位来源编号（由银商分配）开头。
        /// 多次退款必传，每次退款上送的refundOrderId值需不同
        /// </summary>
        public string RefundOrderId { get; set; }
        /// <summary>
        /// 要退货的金额
        /// 全额退款时refundAmount等于支付总金额totalAmount
        /// </summary>
        public int RefundAmount { get; set; }
        /// <summary>
        /// 退货说明
        /// </summary>
        public string RefundDesc { get; set; }
        public ChinaumsPayDictionary GetParameters()
        {
            return new ChinaumsPayDictionary
            {
                {"msgId",MsgId },
                {"msgSrc",MsgSrc },
                {"msgType","bills.refund" },
                {"requestTimestamp",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },//报文请求时间，格式yyyy-MM-dd HH:mm:ss 
                {"srcReserve",SrcReserve },
                {"billDate",BillDate },
                {"billNo",BillNo },
                {"refundOrderId",RefundOrderId },
                {"refundAmount",RefundAmount.ToString() },
                {"refundDesc",RefundDesc }
            };
        }
    }
}
