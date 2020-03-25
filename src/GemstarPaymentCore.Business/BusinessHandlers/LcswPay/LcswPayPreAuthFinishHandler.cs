using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
using Essensoft.AspNetCore.Payment.LcswPay.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.LcswPay
{
    /// <summary>
    /// 利楚商务扫呗预授权完成
    /// </summary>
    public class LcswPayPreAuthFinishHandler: BusinessHandlerBase
    {
        private ILogger _log;
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        private readonly BusinessOption _businessOption;
        public LcswPayPreAuthFinishHandler(ILogger<LcswPayBarcodePayHandler> log, ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options,IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = log;
            _client = client;
            _options = options.Value;
            _businessOption = businessOption.Value;
        }
        protected override string contentFormat => "merchantNo|terminalId|accessToken|terminalTrace|terminalTime|payTrace|payTime|finishAmount|outTradeNo|orderBody|attach";
        protected override int[] contentEncryptedIndexs => new int[] { 0 };
        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                var merchantNo = infos[i++];
                var terminalId = infos[i++];
                var accessToken = infos[i++];
                var terminalTrace = infos[i++];
                var terminalTime = infos[i++];
                var payTrace = infos[i++];
                var payTime = infos[i++];
                var finishAmount = infos[i++];
                var outTradeNo = infos[i++];
                var orderBody = infos[i++];
                var attach = infos[i++];
                //调用预授权完成接口
                var request = new LcswPayPreAuthFinishRequest
                {
                    MerchantNo = merchantNo,
                    TerminalId = terminalId,
                    TerminalTime = terminalTime,
                    TerminalTrace = terminalTrace,
                    PayTrace = payTrace,
                    PayTime = payTime,
                    FinishAmt = Convert.ToInt32(Convert.ToDecimal(finishAmount) * 100).ToString(),
                    OutTradeNo = outTradeNo,
                    OrderBody = orderBody,
                    Attach = attach
                };
                _options.Token = accessToken;
                var response = await _client.ExecuteAsync(request, _options);

                if (!response.IsReturnCodeSuccess)
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "01")
                {
                    return HandleResult.Fail($"错误代码{response.ResultCode};错误描述:{response.ReturnMsg}|{response.PayStatusCode}");
                }
                var resultStr = $"{response.OutTradeNo}|{response.PayType}|{response.ChannelTradeNo}|{response.MerchantName}|{response.EndTime}|{response.Attach}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
