using Essensoft.AspNetCore.Payment.LcswPay.Response;
using Essensoft.AspNetCore.Payment.LcswPay.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Essensoft.AspNetCore.Payment.LcswPay.Request
{
    /// <summary>
    /// 利楚商务扫呗条码预授权请求
    /// </summary>
    public class LcswPayPreAuthBarRequest : ILcswPayResquest<LcswPayPreAuthBarResponse>
    {
        public LcswPayPreAuthBarRequest()
        {
            PayVersion = "110";
            PayType = "000";
            ServiceId = "010";
        }
        /// <summary>
        /// 000自动识别
        /// </summary>
        [JsonProperty("pay_type")]
        public string PayType { get; set; }
        /// <summary>
        /// 接口类型，当前类型010
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
        /// 授权码，客户的付款码
        /// </summary>
        [JsonProperty("auth_no")]
        public string AuthNo { get; set; }
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
        /// 附加数据,原样返回
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
            return "/pos/200/preauth/preAuthBar";
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
                    new LcswPayParaInfo("auth_no",AuthNo),
                    new LcswPayParaInfo("total_fee",TotalFee),
                    new LcswPayParaInfo("order_body",OrderBody,false),
                    new LcswPayParaInfo("attach",Attach,false)
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