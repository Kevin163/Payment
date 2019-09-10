using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.LcswPay
{
    /// <summary>
    /// 扫呗公众号支付统一下单业务请求处理
    /// </summary>
    public class LcswPayJsPayHandler : IBusinessHandler
    {
        private const string contentFormat = "payType|merchantNo|terminalId|accessToken|terminalTrace|terminalTime|totalFee|appId|openId|orderBody|attach|notifyUrl";
        private const char splitChar = '|';
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        private string _businessContent;
        public LcswPayJsPayHandler(ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options)
        {
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
                var payType = infos[i++];
                var merchantNo = infos[i++];
                var terminalId = infos[i++];
                var accessToken = infos[i++];
                var terminalTrace = infos[i++];
                var terminalTime = infos[i++];
                var totalFee = infos[i++];
                var appId = infos[i++];
                var openId = infos[i++];
                var orderBody = infos[i++];
                var attach = infos[i++];
                var notifyUrl = infos[i++];

                var request = new LcswPayJspayRequest
                {
                    PayType = payType,
                    MerchantNo = merchantNo,
                    TerminalId = terminalId,
                    TerminalTime = terminalTime,
                    TerminalTrace = terminalTrace,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(totalFee) * 100).ToString(),
                    SubAppid = appId,
                    OpenId = openId,
                    OrderBody = orderBody,
                    Attach = attach,
                    NotifyUrl = notifyUrl
                };
                _options.Token = accessToken;
                var response = await _client.ExecuteAsync(request, _options);

                if (!response.IsReturnCodeSuccess)
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "01")
                {
                    return HandleResult.Fail($"错误代码{response.ResultCode};错误描述:{response.ReturnMsg}");
                }
                var resultStr = $"{response.OutTradeNo}|{response.AppId}|{response.TimeStamp}|{response.NonceStr}|{response.PackageStr}|{response.WxSignType}|{response.PaySign}|{response.AliTradeNo}|{response.TokenId}";
                return HandleResult.Success(resultStr);
            }
            catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }


    }
}
