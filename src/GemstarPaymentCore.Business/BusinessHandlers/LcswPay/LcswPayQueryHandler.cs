using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.LcswPay
{
    /// <summary>
    /// 利楚商务扫呗支付查询支付结果
    /// </summary>
    public class LcswPayQueryHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        public LcswPayQueryHandler(ILogger<LcswPayQueryHandler> log, ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
        }
        protected override string contentFormat => "payType|merchantNo|terminalId|accessToken|terminalTrace|terminalTime|payTrace|payTime|outTradeNo";
        protected override int[] contentEncryptedIndexs => new int[] { 1 };

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                var payType = infos[i++];
                var merchantNo = infos[i++];
                var terminalId = infos[i++];
                var accessToken = infos[i++];
                var terminalTrace = infos[i++];
                var terminalTime = infos[i++];
                var payTrace = infos[i++];
                var payTime = infos[i++];
                var outTradeNo = infos[i++];
                if (string.IsNullOrEmpty(outTradeNo))
                {
                    if (string.IsNullOrEmpty(payTrace) || string.IsNullOrEmpty(payTime))
                    {
                        return HandleResult.Fail($"没指定订单号参数时，必须同时指定支付终端流水号和支付终端交易时间参数");
                    }
                }
                //调用查询接口
                var request = new LcswPayQueryRequest
                {
                    PayType = payType,
                    ServiceId = "020",
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
                    return HandleResult.Fail($"错误代码{response.ResultCode};错误描述:{response.ReturnMsg}");
                }
                if (response.TradeState != "SUCCESS")
                {
                    return HandleResult.Fail($"当前支付状态{response.TradeState}{response.ReturnMsg}");
                }
                var resultStr = $"{response.OutTradeNo}|{response.ChannelTradeNo}|{response.MerchantName}|{response.EndTime}|{response.Attach}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
