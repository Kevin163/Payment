using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.LcswPay
{
    /// <summary>
    /// 利楚商务扫呗支付退款查询
    /// </summary>
    public class LcswPayRefundQueryHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        public LcswPayRefundQueryHandler(ILogger<LcswPayRefundQueryHandler> log, ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
        }
        protected override string contentFormat => "payType|merchantNo|terminalId|accessToken|terminalTrace|terminalTime|outRefundNo|payTrace|payTime";
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
                var outRefundNo = infos[i++];
                var payTrace = infos[i++];
                var payTime = infos[i++];
                //调用退款申请接口
                var request = new LcswPayRefundQueryRequest
                {
                    PayType = payType,
                    ServiceId = "031",
                    MerchantNo = merchantNo,
                    TerminalId = terminalId,
                    TerminalTime = terminalTime,
                    TerminalTrace = terminalTrace,
                    OutRefundNo = outRefundNo,
                    PayTrace = payTrace,
                    PayTime = payTime
                };
                _options.Token = accessToken;
                var response = await _client.ExecuteAsync(request, _options);

                if (!response.IsReturnCodeSuccess)
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "01")
                {
                    return HandleResult.Fail($"错误代码{response.ResultCode};错误描述:{response.ReturnMsg}|{response.TradeStatus}");
                }
                if (response.TradeStatus != "SUCCESS")
                {
                    return HandleResult.Fail($"退款请求还没有处理成功，当前状态是:{response.TradeStatus}|{response.TradeStatus}");
                }
                var resultStr = $"{response.OutTradeNo}|{response.OutRefundNo}|{response.EndTime}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
