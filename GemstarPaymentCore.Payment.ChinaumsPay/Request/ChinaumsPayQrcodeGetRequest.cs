using System;
using GemstarPaymentCore.Payment.ChinaumsPay.Response;

namespace GemstarPaymentCore.Payment.ChinaumsPay.Request
{
    /// <summary>
    /// 二维码获取接口请求
    /// </summary>
    public class ChinaumsPayQrcodeGetRequest : IChinaumsPayRequest<ChinaumsPayQrcodeGetResponse>
    {
        /// <summary>
        /// 消息ID，原样返回
        /// </summary>
        public string MsgId { get; set; }
        /// <summary>
        /// 来源系统标识
        /// </summary>
        public string MsgSrc { get; set; }
        /// <summary>
        /// 账单号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 账单日期，格式yyyy-MM-dd
        /// </summary>
        public string BillDate { get; set; }
        /// <summary>
        /// 账单描述
        /// </summary>
        public string BillDesc { get; set; }
        /// <summary>
        /// 支付总金额,单位为分
        /// </summary>
        public int TotalAmount { get; set; }

        /// <summary>
        /// 获取请求的所有参数
        /// </summary>
        /// <returns>请求参数数组</returns>
        public ChinaumsPayDictionary GetParameters()
        {
            return new ChinaumsPayDictionary
            {
                {"msgId",MsgId },
                {"msgSrc",MsgSrc },
                {"msgType","bills.getQRCode" },//消息类型
                {"requestTimestamp",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },//报文请求时间，格式yyyy-MM-dd HH:mm:ss
                {"instMid","QRPAYDEFAULT" },
                {"billNo",BillNo },
                {"billDate",BillDate },
                {"billDesc",BillDesc },
                {"totalAmount",TotalAmount }
            };
        }
    }
}
