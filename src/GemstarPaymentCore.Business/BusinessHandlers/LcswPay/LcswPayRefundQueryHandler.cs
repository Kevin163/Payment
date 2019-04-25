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
    public class LcswPayRefundQueryHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "payType|merchantNo|terminalId|accessToken|terminalTrace|terminalTime|outRefundNo|payTrace|payTime";
        private const char splitChar = '|';
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        private string _businessContent;
        public LcswPayRefundQueryHandler(ILogger<LcswPayRefundQueryHandler> log, ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options)
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
                var response = await _client.ExecuteAsync(request,_options);

                if (!response.IsReturnCodeSuccess)
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "01")
                {
                    return HandleResult.Fail($"错误代码{response.ResultCode};错误描述:{response.ReturnMsg}");
                }
                if(response.TradeStatus != "SUCCESS")
                {
                    return HandleResult.Fail($"退款请求还没有处理成功，当前状态是:{response.TradeStatus}");
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
