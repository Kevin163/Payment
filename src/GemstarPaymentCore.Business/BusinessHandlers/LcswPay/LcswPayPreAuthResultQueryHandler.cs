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
    /// 利楚商务扫呗预授权结果查询
    /// 用于查询预授权完成和撤销结果
    /// </summary>
    public class LcswPayPreAuthResultQueryHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        private readonly BusinessOption _businessOption;
        private string _businessContent;
        public LcswPayPreAuthResultQueryHandler(ILogger<LcswPayBarcodePayHandler> log, ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options, IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = log;
            _client = client;
            _options = options.Value;
            _businessOption = businessOption.Value;
        }
        protected override string contentFormat => "orderType|merchantNo|terminalId|accessToken|terminalTrace|terminalTime|payTrace|payTime|outTradeNo";
        protected override int[] contentEncryptedIndexs => new int[] { 1 };

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                var orderType = infos[i++];
                var merchantNo = infos[i++];
                var terminalId = infos[i++];
                var accessToken = infos[i++];
                var terminalTrace = infos[i++];
                var terminalTime = infos[i++];
                var payTrace = infos[i++];
                var payTime = infos[i++];
                var outTradeNo = infos[i++];
                //调用预授权撤销和完成查询接口
                var request = new LcswPayPreAuthPayQueryRequest
                {
                    OrderType = orderType,
                    MerchantNo = merchantNo,
                    TerminalId = terminalId,
                    TerminalTime = terminalTime,
                    TerminalTrace = terminalTrace,
                    PayTrace = payTrace,
                    PayTime = payTime,
                    OutTradeNo = outTradeNo
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
                var resultStr = $"{response.OutTradeNo}|{response.OrigTradeNo}|{response.PayType}|{response.OrderAmt}|{response.FinishAmt}|{response.ReturnAmt}|{response.PayStatusCode}|{response.ChannelTradeNo}|{response.MerchantName}|{response.EndTime}|{response.Attach}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
