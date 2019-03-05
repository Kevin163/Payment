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
    public class AlipayCloseOrderHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "Appid|Pid|outTradeNo";
        private const char splitChar = '|';
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        private string _businessContent;
        public AlipayCloseOrderHandler(ILogger<AlipayCloseOrderHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options)
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
                var Appid = infos[i++];
                var Pid = infos[i++];
                var outTradeNo = infos[i++];

                var model = new AlipayTradeCloseModel
                {
                    OutTradeNo = outTradeNo
                };
                var request = new AlipayTradeCloseRequest();
                request.SetBizModel(model);

                _options.AppId = Appid;
                _options.PId = Pid;
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
