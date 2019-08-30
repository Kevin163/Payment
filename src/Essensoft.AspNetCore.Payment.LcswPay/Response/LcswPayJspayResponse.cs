using Essensoft.AspNetCore.Payment.LcswPay.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Essensoft.AspNetCore.Payment.LcswPay.Response
{
    /// <summary>
    /// 扫呗公众号预支付（统一下单）响应
    /// </summary>
    public class LcswPayJspayResponse:LcswPayResponse
    {
        /// <summary>
        /// 业务结果：01成功 02失败 ，03支付中，99该条码暂不支持支付类型自动匹配
        /// </summary>
        [JsonProperty("result_code")]
        public string ResultCode { get; set; }
        /// <summary>
        /// 支付方式，010微信，020支付宝，060qq钱包，090口碑，100翼支付
        /// </summary>
        [JsonProperty("pay_type")]
        public string PayType { get; set; }
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
        /// 利楚唯一订单号
        /// </summary>
        [JsonProperty("out_trade_no")]
        public string OutTradeNo { get; set; }
        /// <summary>
        /// 微信公众号支付返回字段，公众号id
        /// </summary>
        [JsonProperty("appId")]
        public string AppId { get; set; }
        /// <summary>
        /// 微信公众号支付返回字段，时间戳，示例：1414561699，标准北京时间，时区为东八区，自1970年1月1日 0点0分0秒以来的秒数。注意：部分系统取到的值为毫秒级，需要转换成秒(10位数字)。
        /// </summary>
        [JsonProperty("timeStamp")]
        public string TimeStamp { get; set; }
        /// <summary>
        /// 微信公众号支付返回字段，随机字符串
        /// </summary>
        [JsonProperty("nonceStr")]
        public string NonceStr { get; set; }
        /// <summary>
        /// 微信公众号支付返回字段，订单详情扩展字符串，示例：prepay_id=123456789，统一下单接口返回的prepay_id参数值，提交格式如：prepay_id=
        /// </summary>
        [JsonProperty("package_str")]
        public string PackageStr { get; set; }
        /// <summary>
        /// 微信公众号支付返回字段，签名方式，示例：MD5,RSA
        /// </summary>
        [JsonProperty("signType")]
        public string WxSignType { get; set; }
        /// <summary>
        /// 微信公众号支付返回字段，签名
        /// </summary>
        [JsonProperty("paySign")]
        public string PaySign { get; set; }
        /// <summary>
        /// 支付宝JSAPI支付返回字段用于调起支付宝JSAPI
        /// </summary>
        [JsonProperty("ali_trade_no")]
        public string AliTradeNo { get; set; }
        /// <summary>
        /// qq钱包公众号支付
        /// </summary>
        [JsonProperty("token_id")]
        public string TokenId { get; set; }

        public override void AddSignedParasWhenReturnCodeSuccess(List<LcswPayParaInfo> signedParas)
        {
            signedParas.AddRange(new List<LcswPayParaInfo> {
                new LcswPayParaInfo("result_code",ResultCode),
                new LcswPayParaInfo("pay_type",PayType),
                new LcswPayParaInfo("merchant_name",MerchantName),
                new LcswPayParaInfo("merchant_no",MerchantNo),
                new LcswPayParaInfo("terminal_id",TerminalId),
                new LcswPayParaInfo("terminal_trace",TerminalTrace),
                new LcswPayParaInfo("terminal_time",TerminalTime),
                new LcswPayParaInfo("total_fee",TotalFee),
                new LcswPayParaInfo("out_trade_no",OutTradeNo)
            });
        }
    }
}
