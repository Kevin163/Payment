using Essensoft.AspNetCore.Payment.LcswPay.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Essensoft.AspNetCore.Payment.LcswPay.Response
{
    /// <summary>
    /// 利楚商务扫呗预授权完成响应
    /// </summary>
    public class LcswPayPreAuthFinishResponse : LcswPayResponse
    {
        /// <summary>
        /// 业务结果：01成功 ，02失败
        /// </summary>
        [JsonProperty("result_code")]
        public string ResultCode { get; set; }
        /// <summary>
        /// 请求类型，010微信，020 支付宝
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
        [JsonProperty("order_amt")]
        public string OrderAmt { get; set; }
        /// <summary>
        /// 预授权完成金额
        /// </summary>
        [JsonProperty("finish_amt")]
        public string FinishAmt { get; set; }
        /// <summary>
        /// 预授权完成退回金额
        /// </summary>
        [JsonProperty("return_amt")]
        public string ReturnAmt { get; set; }
        /// <summary>
        /// 手续费
        /// </summary>
        [JsonProperty("poundage")]
        public string Poundage { get; set; }
        /// <summary>
        /// 支付完成时间，yyyyMMddHHmmss，全局统一时间格式
        /// </summary>
        [JsonProperty("end_time")]
        public string EndTime { get; set; }
        /// <summary>
        /// 利楚唯一订单号
        /// </summary>
        [JsonProperty("out_trade_no")]
        public string OutTradeNo { get; set; }
        /// <summary>
        /// 撤销原订单号
        /// </summary>
        [JsonProperty("orig_trade_no")]
        public string OrigTradeNo { get; set; }
        /// <summary>
        /// 通道订单号，微信订单号、支付宝订单号等
        /// </summary>
        [JsonProperty("channel_trade_no")]
        public string ChannelTradeNo { get; set; }
        /// <summary>
        /// 银行渠道订单号，微信支付时显示在支付成功页面的条码，可用作扫码查询和扫码退款时匹配
        /// </summary>
        [JsonProperty("channel_order_no")]
        public string ChannelOrderNo { get; set; }   
        /// <summary>
        /// 用户id
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { get; set; }     
        /// <summary>
        /// 订单描述
        /// </summary>
        [JsonProperty("order_body")]
        public string OrderBody { get; set; }
        /// <summary>
        /// 02预授权撤销 03预授权完成
        /// </summary>
        [JsonProperty("order_type")]
        public string OrderType { get; set; }
        /// <summary>
        /// 附加数据,原样返回
        /// </summary>
        [JsonProperty("attach")]
        public string Attach { get; set; }
        /// <summary>
        /// 交易状态 “7”，预授权成功 “3” 预授权支付中
        /// </summary>
        [JsonProperty("pay_status_code")]
        public string PayStatusCode { get; set; }
        /// <summary>
        /// 门店名称
        /// </summary>
        [JsonProperty("store_name")]
        public string StoreName { get; set; }

        public override LcswPayResponseSignType SignType => LcswPayResponseSignType.AllNotNullParas;
        public override bool CalcSignNeedToken => true;


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
                new LcswPayParaInfo("order_amt",OrderAmt),
                new LcswPayParaInfo("finish_amt",FinishAmt),
                new LcswPayParaInfo("return_amt",ReturnAmt),
                new LcswPayParaInfo("poundage",Poundage),
                new LcswPayParaInfo("end_time",EndTime),
                new LcswPayParaInfo("out_trade_no",OutTradeNo),
                new LcswPayParaInfo("orig_trade_no",OrigTradeNo),
                new LcswPayParaInfo("channel_trade_no",ChannelTradeNo),
                new LcswPayParaInfo("channel_order_no",ChannelOrderNo),
                new LcswPayParaInfo("user_id",UserId),
                new LcswPayParaInfo("order_body",OrderBody),
                new LcswPayParaInfo("attach",Attach),
                new LcswPayParaInfo("pay_status_code",PayStatusCode),
                new LcswPayParaInfo("order_type",OrderType),
                new LcswPayParaInfo("store_name",StoreName)
            });
        }
    }
}
