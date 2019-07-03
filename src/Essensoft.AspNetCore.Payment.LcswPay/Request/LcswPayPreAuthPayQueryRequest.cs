using Essensoft.AspNetCore.Payment.LcswPay.Response;
using Essensoft.AspNetCore.Payment.LcswPay.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Essensoft.AspNetCore.Payment.LcswPay.Request
{
    /// <summary>
    /// 利楚商务扫呗预授权支付查询
    /// 用于查询预授权完成和预授权撤销结果
    /// </summary>
    public class LcswPayPreAuthPayQueryRequest : ILcswPayResquest<LcswPayPreAuthPayQueryResponse>
    {
        public LcswPayPreAuthPayQueryRequest()
        {
            PayVersion = "110";
            ServiceId = "017";
        }
        /// <summary>
        /// 02预授权撤销订单查询 03预授权完成订单查询
        /// </summary>
        [JsonProperty("order_type")]
        public string OrderType { get; set; }
        /// <summary>
        /// 接口类型，当前类型017
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
        /// 附加数据，原样返回
        /// </summary>
        [JsonProperty("attach")]
        public string Attach { get; set; }
        /// <summary>
        /// 预授权完成或撤销订单号，查询凭据
        /// </summary>
        [JsonProperty("out_trade_no")]
        public string OutTradeNo { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty("order_body")]
        public string OrderBody { get; set; }

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
            return "/pos/200/preauth/preAuthQuery";
        }

        public LcswPaySignInfo GetSignInfo()
        {
            return new LcswPaySignInfo
            {
                SignType = LcswPaySignType.AllNotNullParaAndToken,
                ParaInfos = new List<LcswPayParaInfo>
                {
                    new LcswPayParaInfo("pay_ver",PayVersion),
                    new LcswPayParaInfo("order_type",OrderType),
                    new LcswPayParaInfo("service_id",ServiceId),
                    new LcswPayParaInfo("merchant_no",MerchantNo),
                    new LcswPayParaInfo("terminal_id",TerminalId),
                    new LcswPayParaInfo("terminal_trace",TerminalTrace),
                    new LcswPayParaInfo("terminal_time",TerminalTime),
                    new LcswPayParaInfo("pay_trace",PayTrace,false),
                    new LcswPayParaInfo("pay_time",PayTime,false),
                    new LcswPayParaInfo("attach",Attach),
                    new LcswPayParaInfo("out_trade_no",OutTradeNo),
                    new LcswPayParaInfo("order_body",OrderBody)
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