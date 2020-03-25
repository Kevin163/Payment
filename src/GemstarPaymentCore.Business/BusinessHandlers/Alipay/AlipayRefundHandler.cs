using System;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.Alipay;
using Essensoft.AspNetCore.Payment.Alipay.Domain;
using Essensoft.AspNetCore.Payment.Alipay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GemstarPaymentCore.Business.BusinessHandlers.Alipay
{
    /// <summary>
    /// 支付宝支付退款接口
    /// </summary>
    public class AlipayRefundHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        public AlipayRefundHandler(ILogger<AlipayRefundHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
        }
        protected override string contentFormat => "trade_no|refund_amount|refund_reason|out_request_no|AppId";
        protected override int[] contentEncryptedIndexs => new int[] { 4 };

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                var trade_no = infos[i++];
                var refund_amount = infos[i++];
                var refund_reason = infos[i++];
                var out_request_no = infos[i++];
                if (i < infos.Length)
                {
                    _options.AppId = infos[i++];
                }
                if (i < infos.Length)
                {
                    _options.PId = infos[i++];
                }
                if (i < infos.Length)
                {
                    var temp = infos[i++];
                    if (!string.IsNullOrEmpty(temp))
                    {
                        _options.RsaPublicKey = temp;
                    }
                }
                if (i < infos.Length)
                {
                    var temp = infos[i++];
                    if (!string.IsNullOrEmpty(temp))
                    {
                        _options.RsaPrivateKey = temp;
                    }
                }
                if (i < infos.Length)
                {
                    var temp = infos[i++];
                    if (!string.IsNullOrEmpty(temp))
                    {
                        _options.SignType = temp;
                    }
                }
                if (string.IsNullOrEmpty(_options.AppId))
                {
                    return HandleResult.Fail("请指定支付宝收款账号信息");
                }
                if (string.IsNullOrEmpty(_options.RsaPublicKey) || string.IsNullOrEmpty(_options.RsaPrivateKey))
                {
                    return HandleResult.Fail("请指定支付宝对应的密钥信息");
                }

                refund_amount = Convert.ToDouble(refund_amount).ToString("0.00");
#if MOCK
            //如果定义了模拟编译变量，则直接根据金额来返回一个固定的结果，金额小于5则返回失败，金额大于等于5则直接返回支付成功
            if (builder.trade_no != "mocktransno")
            {
                return HandleResult.Fail("Mock测试时，只能针对mocktransno交易号进行发起退款");
            }
            else
            {
                return HandleResult.Success("mockrefundno|19000101000000|" + builder.refund_amount);
            }
#endif
                var model = new AlipayTradeRefundModel
                {
                    TradeNo = trade_no,
                    RefundAmount = refund_amount,
                    RefundReason = refund_reason,
                    OutRequestNo = out_request_no
                };
                var request = new AlipayTradeRefundRequest();
                request.SetBizModel(model);

                var response = await _client.ExecuteAsync(request, _options);
                var result = response.FailResult();
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
                    var resultStr = string.Format("{0}|{1}|{2}", tradeNo, paidDate, paidAmount);
                    result = HandleResult.Success(resultStr);
                }
                return result;
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
