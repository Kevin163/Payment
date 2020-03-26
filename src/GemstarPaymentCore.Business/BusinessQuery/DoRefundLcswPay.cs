using System;
using Essensoft.AspNetCore.Payment.LcswPay;
using GemstarPaymentCore.Data;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessQuery
{
    /// <summary>
    /// 扫呗退款
    /// </summary>
    public class DoRefundLcswPay : DoRefundBase
    {
        public override async Task<RefundResult> DoRefund(WaitRefundList record, IServiceProvider serviceProvider)
        {
            var client = serviceProvider.GetService<ILcswPayClient>();
            var options = serviceProvider.GetService<IOptionsSnapshot<LcswPayOption>>().Value;
            //转换json参数
            var para = JsonConvert.DeserializeObject<RefundPara>(record.RefundPara);
            //调用退款申请接口
            var request = new LcswPayRefundRequest
            {
                PayType = para.PayType,
                ServiceId = "030",
                MerchantNo = para.MerchantNo,
                TerminalId = para.TerminalId,
                TerminalTime = para.TerminalTime,
                TerminalTrace = para.TerminalTrace,
                RefundFee = Convert.ToInt32(Convert.ToDecimal(para.RefundFee) * 100).ToString(),
                OutTradeNo = para.OutTradeNo,
                PayTrace = para.PayTrace,
                PayTime = para.PayTime,
                AuthCode = para.AuthCode
            }; 
            if (request.MerchantNo.Length > 16)
            {
                request.MerchantNo = Security.Decrypt(request.MerchantNo, SeriesNo);
            }
            options.Token = para.AccessToken;
            var response = await client.ExecuteAsync(request, options);

            var result = new RefundResult
            {
                RefundStatu = WaitRefundList.RefundStatu.StatuSuccess,
                RefundFailReason = ""
            };
            if (!response.IsReturnCodeSuccess)
            {
                result.RefundStatu = WaitRefundList.RefundStatu.StatuFail;
                result.RefundFailReason = response.ReturnMsg;
            }
            if (response.ResultCode != "01")
            {
                result.RefundStatu = response.ResultCode;
                result.RefundFailReason = response.ReturnMsg;
            }
            return result;
        }
        public class RefundPara
        {
            /// <summary>
            /// 支付类型
            /// </summary>
            public string PayType { get; set; }
            /// <summary>
            /// 商户号
            /// </summary>
            public string MerchantNo { get; set; }
            /// <summary>
            /// 终端号
            /// </summary>
            public string TerminalId { get; set; }
            /// <summary>
            /// 终端令牌
            /// </summary>
            public string AccessToken { get; set; }
            /// <summary>
            /// 退款申请单号
            /// </summary>
            public string TerminalTrace { get; set; }
            /// <summary>
            /// 本次退款时间
            /// </summary>
            public string TerminalTime { get; set; }
            /// <summary>
            /// 退款金额
            /// </summary>
            public string RefundFee { get; set; }
            /// <summary>
            /// 扫呗唯一订单号
            /// </summary>
            public string OutTradeNo { get; set; }
            /// <summary>
            /// 支付单号
            /// </summary>
            public string PayTrace { get; set; }
            /// <summary>
            /// 支付时间
            /// </summary>
            public string PayTime { get; set; }
            /// <summary>
            /// 短信或邮箱验证码
            /// </summary>
            public string AuthCode { get; set; }
        }
    }
}
