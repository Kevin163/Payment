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
    /// 支付宝支付关闭订单
    /// </summary>
    public class AlipayCloseOrderHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        public AlipayCloseOrderHandler(ILogger<AlipayCloseOrderHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
        }
        protected override string contentFormat => "Appid|Pid|outTradeNo";
        protected override int[] contentEncryptedIndexs => new int[] { 0, 1 };

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                if (i < infos.Length)
                {
                    _options.AppId = infos[i++];
                }
                if (i < infos.Length)
                {
                    _options.PId = infos[i++];
                }
                var outTradeNo = infos[i++];
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

                var model = new AlipayTradeCloseModel
                {
                    OutTradeNo = outTradeNo
                };
                var request = new AlipayTradeCloseRequest();
                request.SetBizModel(model);

                var response = await _client.ExecuteAsync(request, _options);

                var result = response.FailResult();

                if (response.IsSuccessCode())
                {
                    return HandleResult.Success(response.Msg);
                }
                return result;
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
