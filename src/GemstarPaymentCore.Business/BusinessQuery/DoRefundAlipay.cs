using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.Alipay;
using GemstarPaymentCore.Data;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Essensoft.AspNetCore.Payment.Alipay.Domain;
using Essensoft.AspNetCore.Payment.Alipay.Request;
using GemstarPaymentCore.Business.BusinessHandlers.Alipay;

namespace GemstarPaymentCore.Business.BusinessQuery
{
    /// <summary>
    /// 支付宝退款
    /// </summary>
    public class DoRefundAlipay : DoRefundBase
    {
        public override async Task<RefundResult> DoRefund(WaitRefundList record, IServiceProvider serviceProvider)
        {
            var client = serviceProvider.GetService<IAlipayClient>();
            var options = serviceProvider.GetService<IOptionsSnapshot<AlipayOptions>>().Value;
            var para = JsonConvert.DeserializeObject<RefundPara>(record.RefundPara);
            var trade_no = para.TradeNo;
            var refund_amount = para.RefundAmount;
            var refund_reason = para.RefundReason;
            var out_request_no = para.OutRequestNo;
            options.AppId = para.AppId;
            var result = new RefundResult
            {
                RefundStatu = WaitRefundList.RefundStatu.StatuFail,
                RefundFailReason = ""
            };
            if (string.IsNullOrEmpty(options.AppId))
            {
                result.RefundFailReason = "请指定支付宝收款账号信息";
                return result;
            }
            if (string.IsNullOrEmpty(options.RsaPublicKey) || string.IsNullOrEmpty(options.RsaPrivateKey))
            {
                result.RefundFailReason = "请指定支付宝对应的密钥信息";
                return result;
            }

            refund_amount = Convert.ToDouble(refund_amount).ToString("0.00");

            var model = new AlipayTradeRefundModel
            {
                TradeNo = trade_no,
                RefundAmount = refund_amount,
                RefundReason = refund_reason,
                OutRequestNo = out_request_no
            };
            var request = new AlipayTradeRefundRequest();
            request.SetBizModel(model);

            var response = await client.ExecuteAsync(request, options);
            var aliResult = response.FailResult();
            if (response.IsSuccessCode())
            {
                var paidAmount = refund_amount;
                var tradeNo = "";
                var paidDate = DateTime.Now.ToString("yyyyMMddHHmmss");
                if (response != null)
                {
                    paidAmount = response.RefundFee;
                    tradeNo = response.TradeNo;
                    paidDate = DateTime.Parse(response.GmtRefundPay).ToString("yyyyMMddHHmmss");
                }
                result.RefundStatu = WaitRefundList.RefundStatu.StatuSuccess;
                result.RefundFailReason = "";
                return result;
            }
            result.RefundFailReason = aliResult.Content;
            return result;
        }
        public class RefundPara
        {
            /// <summary>
            /// 支付流水号
            /// </summary>
            [JsonProperty("trade_no")]
            public string TradeNo { get; set; }
            /// <summary>
            /// 退款金额
            /// </summary>
            [JsonProperty("refund_amount")]
            public string RefundAmount { get; set; }
            /// <summary>
            /// 退款原因
            /// </summary>
            [JsonProperty("refund_reason")]
            public string RefundReason { get; set; }
            /// <summary>
            /// 退款申请单号
            /// </summary>
            [JsonProperty("out_request_no")]
            public string OutRequestNo { get; set; }
            /// <summary>
            /// 支付宝appid
            /// </summary>
            [JsonProperty("AppId")]
            public string AppId { get; set; }
        }
    }
}
