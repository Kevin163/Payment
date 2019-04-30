using Essensoft.AspNetCore.Payment.LcswPay.Response;
using Essensoft.AspNetCore.Payment.LcswPay.Utility;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Essensoft.AspNetCore.Payment.LcswPay.Request
{
    /// <summary>
    /// 利楚商务扫呗支付聚合二维码支付请求
    /// </summary>
    public class LcswPayUnionQrcodePayRequest : ILcswPayResquest<LcswPayUnionQrcodePayResponse>
    {
        public LcswPayUnionQrcodePayRequest()
        {
            PayType = "000";
            PayVersion = "110";
            ServiceId = "016";
        }
        /// <summary>
        /// 请求类型，000
        /// </summary>
        [JsonProperty("pay_type")]
        public string PayType { get; set; }
        /// <summary>
        /// 接口类型，当前类型016
        /// </summary>
        [JsonProperty("service_id")]
        public string ServiceId { get; set; }
        /// <summary>
        /// 终端流水号，填写商户系统的订单号
        /// </summary>
        [JsonProperty("terminal_trace")]
        public string TerminalTrace { get; set; }
        /// <summary>
        /// 终端交易时间，yyyyMMddHHmmss，全局统一时间格式
        /// </summary>
        [JsonProperty("terminal_time")]
        public string TerminalTime { get; set; }
        /// <summary>
        /// 金额，单位分
        /// </summary>
        [JsonProperty("total_fee")]
        public string TotalFee { get; set; }
        /// <summary>
        /// 订单描述
        /// </summary>
        [JsonProperty("order_body")]
        public string OrderBody { get; set; }
        /// <summary>
        /// 外部系统通知地址
        /// </summary>
        [JsonProperty("notify_url")]
        public string NotifyUrl { get; set; }
        /// <summary>
        /// 附加数据，原样返回
        /// </summary>
        [JsonProperty("attach")]
        public string Attach { get; set; }

        #region ILcswPayResquest Members
        /// <summary>
        /// 版本号，当前版本110
        /// </summary>
        [JsonProperty("pay_ver")]
        public string PayVersion { get; set; }
        /// <summary>
        /// 商户号
        /// </summary>
        [JsonProperty("merchant_no")]
        public string MerchantNo { get; set; }
        /// <summary>
        /// 终端号
        /// </summary>
        [JsonProperty("terminal_id")]
        public string TerminalId { get; set; }

        public string GetRequestUrl()
        {
            return "/pay/110/qrpay";
        }

        public LcswPaySignInfo GetSignInfo()
        {
            return new LcswPaySignInfo
            {
                SignType = LcswPaySignType.AllRequiredParaAndToken,
                ParaInfos = new List<LcswPayParaInfo>
                {
                    //这一个是按字典序，所以这里需要先把参数提成好序
                    new LcswPayParaInfo("attach",Attach),
                    new LcswPayParaInfo("merchant_no",MerchantNo),
                    new LcswPayParaInfo("notify_url",NotifyUrl),
                    new LcswPayParaInfo("order_body",OrderBody),
                    new LcswPayParaInfo("pay_type",PayType),
                    new LcswPayParaInfo("pay_ver",PayVersion),
                    new LcswPayParaInfo("service_id",ServiceId),
                    new LcswPayParaInfo("terminal_id",TerminalId),
                    new LcswPayParaInfo("terminal_time",TerminalTime),
                    new LcswPayParaInfo("terminal_trace",TerminalTrace),
                    new LcswPayParaInfo("total_fee",TotalFee)
                }
            };
        }

        public bool IsCheckResponseSign()
        {
            return true;
        }
        #endregion
    }
}
