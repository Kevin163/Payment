using Essensoft.AspNetCore.Payment.LcswPay.Response;
using Essensoft.AspNetCore.Payment.LcswPay.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Essensoft.AspNetCore.Payment.LcswPay.Request
{
    /// <summary>
    /// 扫呗公众号预支付（统一下单）请求
    /// </summary>
    public class LcswPayJspayRequest : ILcswPayResquest<LcswPayJspayResponse>
    {
        public LcswPayJspayRequest()
        {
            ServiceId = "012";
        }

        /// <summary>
        /// 支付方式，010微信，020支付宝，060qq钱包，090口碑，100翼支付，140和包支付（仅限和包通道）
        /// </summary>
        [JsonProperty("pay_type")]
        public string PayType { get; set; }
        /// <summary>
        /// 接口类型，当前类型012
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
        /// 公众号appid，公众号支付时使用的appid（若传入，则open_id需要保持一致）
        /// </summary>
        [JsonProperty("sub_appid")]
        public string SubAppid { get; set; }
        /// <summary>
        /// 用户标识（微信openid，支付宝userid），pay_type为010及020时需要传入
        /// </summary>
        [JsonProperty("open_id")]
        public string OpenId { get; set; }
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
        /// 附加数据,原样返回
        /// </summary>
        [JsonProperty("attach")]
        public string Attach { get; set; }
        /// <summary>
        /// 订单包含的商品列表信息，Json格式。pay_type为090时，可选填此字段
        /// 每个商品的信息可参考GoodInfo
        /// </summary>
        [JsonProperty("goods_detail")]
        public string GoodsDetail { get; set; }
        /// <summary>
        /// 订单优惠标记，代金券或立减优惠功能的参数（字段值：cs和bld）
        /// </summary>
        [JsonProperty("goods_tag")]
        public string GoodsTag { get; set; }

        #region ILcswPayResquest Members
        /// <summary>
        /// 版本号，当前版本100
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
            return "/pay/100/jspay";
        }

        public LcswPaySignInfo GetSignInfo()
        {
            return new LcswPaySignInfo
            {
                SignType = LcswPaySignType.AllRequiredParaAndToken,
                ParaInfos = new List<LcswPayParaInfo>
                {
                    new LcswPayParaInfo("pay_ver",PayVersion),
                    new LcswPayParaInfo("pay_type",PayType),
                    new LcswPayParaInfo("service_id",ServiceId),
                    new LcswPayParaInfo("merchant_no",MerchantNo),
                    new LcswPayParaInfo("terminal_id",TerminalId),
                    new LcswPayParaInfo("terminal_trace",TerminalTrace),
                    new LcswPayParaInfo("terminal_time",TerminalTime),
                    new LcswPayParaInfo("total_fee",TotalFee),
                    new LcswPayParaInfo("sub_appid",SubAppid,false),
                    new LcswPayParaInfo("open_id",OpenId,false),
                    new LcswPayParaInfo("order_body",OrderBody,false),
                    new LcswPayParaInfo("notify_url",NotifyUrl,false),
                    new LcswPayParaInfo("attach",Attach,false),
                    new LcswPayParaInfo("goods_detail",GoodsDetail,false),
                    new LcswPayParaInfo("goods_tag",GoodsTag,false)
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
