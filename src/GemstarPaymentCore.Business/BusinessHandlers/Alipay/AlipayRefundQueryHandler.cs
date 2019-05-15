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
    /// 支付宝退款查询接口
    /// </summary>
    public class AlipayRefundQueryHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "trade_no|out_request_no|AppId";
        private const char splitChar = '|';
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        private string _businessContent;
        public AlipayRefundQueryHandler(ILogger<AlipayRefundQueryHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
        }

        public void SetBusinessContent(string businessContent)
        {
            _businessContent = businessContent;
        }

        public async Task<HandleResult> HandleBusinessContentAsync()
        {
            //参数有效性检查
            if (string.IsNullOrWhiteSpace(_businessContent))
            {
                return HandleResult.Fail($"必须以格式'{contentFormat}'进行交互");
            }
            var length = contentFormat.Split(splitChar).Length;
            var infos = _businessContent.Split(splitChar);
            if (infos.Length < length)
            {
                return HandleResult.Fail($"必须以格式'{contentFormat}'进行交互");
            }
            try
            {
                int i = 0;
                var trade_no = infos[i++];
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
                    _options.RsaPublicKey = infos[i++];
                }
                if (i < infos.Length)
                {
                    _options.RsaPrivateKey = infos[i++];
                }
                if (i < infos.Length)
                {
                    _options.SignType = infos[i++];
                }
                if (string.IsNullOrEmpty(_options.AppId))
                {
                    return HandleResult.Fail("请指定支付宝收款账号信息");
                }
                if (string.IsNullOrEmpty(_options.RsaPublicKey) || string.IsNullOrEmpty(_options.RsaPrivateKey))
                {
                    return HandleResult.Fail("请指定支付宝对应的密钥信息");
                }
#if MOCK
            //如果定义了模拟编译变量，则直接根据金额来返回一个固定的结果，金额小于5则返回失败，金额大于等于5则直接返回支付成功
            if (para.trade_no != "mocktransno")
            {
                return HandleResult.Fail("Mock测试时，只能针对mocktransno交易号进行发起退款");
            }
            else
            {
                return HandleResult.Success("mockrefundno|19000101000000|" + builder.refund_amount);
            }
#endif
                var model = new AlipayTradeFastpayRefundQueryModel
                {
                    TradeNo = trade_no,
                    OutRequestNo = out_request_no
                };

                var request = new AlipayTradeFastpayRefundQueryRequest();
                request.SetBizModel(model);

                var response = await _client.ExecuteAsync(request,_options);
                var result = response.FailResult();
                if (!response.IsError && !string.IsNullOrWhiteSpace(response.RefundAmount))
                {
                    var paidAmount = response.RefundAmount;
                    var tradeNo = response.TradeNo;
                    var paidDate = DateTime.Now.ToString("yyyyMMddHHmmss");
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

