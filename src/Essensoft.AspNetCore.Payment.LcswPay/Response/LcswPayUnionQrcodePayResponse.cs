using Essensoft.AspNetCore.Payment.LcswPay.Utility;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Essensoft.AspNetCore.Payment.LcswPay.Response
{
    /// <summary>
    /// 利楚商务扫呗支付聚合二维码支付响应
    /// </summary>
    public class LcswPayUnionQrcodePayResponse : LcswPayResponse
    {
        /// <summary>
        /// 业务结果：01成功 ，02失败
        /// </summary>
        [JsonProperty("result_code")]
        public string ResultCode { get; set; }
        /// <summary>
        /// 商户名称
        /// </summary>
        [JsonProperty("merchant_name")]
        public string MerchantName { get; set; }
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
        /// <summary>
        /// 终端流水号，商户系统的订单号，扫呗系统原样返回
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
        /// 二维码码串
        /// </summary>
        [JsonProperty("qr_url")]
        public string QrCode { get; set; }

        public override bool CalcSignNeedToken => true;
        public override void AddSignedParasWhenReturnCodeSuccess(List<LcswPayParaInfo> signedParas)
        {
            //这一个是按字典序，所以这里需要先把参数提成好序,并且清空原来父类中加入的元素，在子类中一起来重新加入
            signedParas.Clear();
            signedParas.AddRange(new List<LcswPayParaInfo> {
                new LcswPayParaInfo("merchant_name",MerchantName),
                new LcswPayParaInfo("merchant_no",MerchantNo),
                new LcswPayParaInfo("qr_url",QrCode),
                new LcswPayParaInfo("result_code",ResultCode),
                new LcswPayParaInfo("return_code", ReturnCode),
                new LcswPayParaInfo("return_msg", ReturnMsg),
                new LcswPayParaInfo("terminal_id",TerminalId),
                new LcswPayParaInfo("terminal_time",TerminalTime),
                new LcswPayParaInfo("terminal_trace",TerminalTrace),
                new LcswPayParaInfo("total_fee",TotalFee)
            });
        }
    }
}
