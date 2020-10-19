using System;
using GemstarPaymentCore.Payment.ChinaumsPay.Response;

namespace GemstarPaymentCore.Payment.ChinaumsPay.Request
{
    /// <summary>
    /// 银商支付二维码关闭请求
    /// </summary>
    public class ChinaumsPayQrcodeCloseRequest : IChinaumsPayRequest<ChinaumsPayQrcodeCloseResponse>
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
        /// 二维码ID,取自获取二维码的返回结果
        /// </summary>
        public string QrCodeId { get; set; }
        public ChinaumsPayDictionary GetParameters()
        {
            return new ChinaumsPayDictionary
            {
                {"msgId",MsgId },
                {"msgSrc",MsgSrc },
                {"msgType","bills.closeQRCode" },
                {"requestTimestamp",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },//报文请求时间，格式yyyy-MM-dd HH:mm:ss
                {"qrCodeId",QrCodeId }
            };
        }
    }
}
