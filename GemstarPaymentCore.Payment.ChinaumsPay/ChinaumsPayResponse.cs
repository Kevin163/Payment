using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GemstarPaymentCore.Payment.ChinaumsPay
{
    /// <summary>
    /// 银商支付响应
    /// </summary>
    public class ChinaumsPayResponse
    {
        /// <summary>
        /// 平台错误码
        /// </summary>
        [JsonProperty("errCode")]
        public string ErrCode { get; set; }
        /// <summary>
        /// 平台错误信息
        /// </summary>
        [JsonProperty("errMsg")]
        public string ErrMsg { get; set; }
        /// <summary>
        /// 消息ID，原样返回
        /// </summary>
        [JsonProperty("msgId")]
        public string MsgId { get; set; }
        /// <summary>
        /// 消息类型，原样返回
        /// </summary>
        [JsonProperty("msgType")]
        public string MsgType { get; set; }
        /// <summary>
        /// 消息来源，原样返回
        /// </summary>
        [JsonProperty("msgSrc")]
        public string MsgSrc { get; set; }
        /// <summary>
        /// 报文应答时间，格式yyyy-MM-dd HH:mm:ss
        /// </summary>
        [JsonProperty("responseTimeStamp")]
        public string ResponseTimeStamp { get; set; }
        /// <summary>
        /// 请求系统预留字段
        /// </summary>
        public string SrcReserve { get; set; }
        /// <summary>
        /// 商户号，原样返回
        /// </summary>
        public string Mid { get; set; }
        /// <summary>
        /// 终端号，原样返回
        /// </summary>
        public string Tid { get; set; }
        /// <summary>
        /// 业务类型，原样返回
        /// </summary>
        public string InstMid { get; set; }
        /// <summary>
        /// 系统ID，原样返回
        /// </summary>
        public string SystemId { get; set; }
        /// <summary>
        /// 签名算法
        /// </summary>
        public string SignType { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        public string Sign { get; set; }
    }
}
