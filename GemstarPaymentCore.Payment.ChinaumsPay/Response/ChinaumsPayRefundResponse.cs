namespace GemstarPaymentCore.Payment.ChinaumsPay.Response
{
    /// <summary>
    /// 银商支付退款响应
    /// </summary>
    public class ChinaumsPayRefundResponse : ChinaumsPayResponse
    {
        /// <summary>
        /// 账单时间，格式 yyyy-MM-dd
        /// </summary>
        public string BillDate { get; set; }
        /// <summary>
        /// 账单号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 账单二维码
        /// </summary>
        public string BillQRCode { get; set; }
        /// <summary>
        /// 账单状态
        /// </summary>
        public string BillStatus { get; set; }
        /// <summary>
        /// 原订单商户订单号
        /// </summary>
        public string MerOrderId { get; set; }
        /// <summary>
        /// 退货订单号
        /// </summary>
        public string RefundOrderId { get; set; }
        /// <summary>
        /// 目标系统退货订单号
        /// </summary>
        public string RefundTargetOrderId { get; set; }
        /// <summary>
        /// 退款时间
        /// </summary>
        public string RefundPayTime { get; set; }
        /// <summary>
        /// 退款结果
        /// SUCCESS成功
        /// FAIL失败
        /// PROCESSING处理中
        /// UNKNOWN异常
        /// </summary>
        public string RefundStatus { get; set; }
        /// <summary>
        /// 借贷记标识
        /// DEBIT_CARD（借记卡）；CREDIT_CARD（贷记卡）
        /// </summary>
        public string CardAttr { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        public long RefundAmount { get; set; }
        /// <summary>
        /// 买家实退金额
        /// </summary>
        public long RefundInvoiceAmount { get; set; }
    }
}
