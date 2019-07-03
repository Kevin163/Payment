using Essensoft.AspNetCore.Payment.LcswPay.Response;
using Essensoft.AspNetCore.Payment.LcswPay.Utility;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Essensoft.AspNetCore.Payment.LcswPay.Request
{
    /// <summary>
    /// 利楚商务扫呗预授权订单查询请求
    /// 用于查询条码冻结订单和聚合二维码订单信息
    /// </summary>
    public class LcswPayPreAuthOrderQueryRequest : ILcswPayResquest<LcswPayPreAuthOrderQueryResponse>
    {
        public LcswPayPreAuthOrderQueryRequest()
        {
            PayVersion = "110";
            ServiceId = "014";
        }
        /// <summary>
        /// 请求类型，010微信，020 支付宝
        /// </summary>
        [JsonProperty("pay_type")]
        public string PayType { get; set; }
        /// <summary>
        /// 接口类型，当前类型014
        /// </summary>
        [JsonProperty("service_id")]
        public string ServiceId { get; set; }
        /// <summary>
        /// 终端查询流水号，填写商户系统的查询流水号
        /// </summary>
        [JsonProperty("terminal_trace")]
        public string TerminalTrace { get; set; }
        /// <summary>
        /// 终端查询时间，yyyyMMddHHmmss，全局统一时间格式
        /// </summary>
        [JsonProperty("terminal_time")]
        public string TerminalTime { get; set; }
        /// <summary>
        /// 当前支付终端流水号，与pay_time同时传递
        /// </summary>
        [JsonProperty("pay_trace")]
        public string PayTrace { get; set; }
        /// <summary>
        /// 当前支付终端交易时间，yyyyMMddHHmmss，全局统一时间格式，与pay_trace同时传递
        /// </summary>
        [JsonProperty("pay_time")]
        public string PayTime { get; set; }
        /// <summary>
        /// 预授权原订单号，查询凭据，可填利楚订单号、微信订单号、支付宝订单号、银行卡订单号任意一个
        /// </summary>
        [JsonProperty("out_trade_no")]
        public string OutTradeNo { get; set; }

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
            return "/pos/200/preauth/commonQuery";
        }

        public LcswPaySignInfo GetSignInfo()
        {
            return new LcswPaySignInfo
            {
                SignType = LcswPaySignType.AllNotNullParaAndToken,
                ParaInfos = new List<LcswPayParaInfo>
                {
                    new LcswPayParaInfo("pay_ver",PayVersion),
                    new LcswPayParaInfo("pay_type",PayType),
                    new LcswPayParaInfo("service_id",ServiceId),
                    new LcswPayParaInfo("merchant_no",MerchantNo),
                    new LcswPayParaInfo("terminal_id",TerminalId),
                    new LcswPayParaInfo("terminal_trace",TerminalTrace),
                    new LcswPayParaInfo("terminal_time",TerminalTime),
                    new LcswPayParaInfo("pay_trace",PayTrace,false),
                    new LcswPayParaInfo("pay_time",PayTime,false),
                    new LcswPayParaInfo("out_trade_no",OutTradeNo)
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