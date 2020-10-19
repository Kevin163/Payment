namespace GemstarPaymentCore.Payment.ChinaumsPay.Response
{
    /// <summary>
    /// 银商获取支付二维码响应
    /// </summary>
    public class ChinaumsPayQrcodeGetResponse : ChinaumsPayResponse
    {
        /// <summary>
        /// 账单号，原样返回
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 账单日期，原样返回
        /// </summary>
        public string BillDate { get; set; }
        /// <summary>
        /// 二维码id,用于后续的二维码关单使用
        /// </summary>
        public string QrCodeId { get; set; }
        /// <summary>
        /// 账单二维码
        /// </summary>
        public string BillQRCode { get; set; }
    }
}
