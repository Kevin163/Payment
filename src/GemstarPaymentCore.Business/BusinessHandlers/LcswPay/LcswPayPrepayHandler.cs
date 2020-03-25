using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.LcswPay
{
    /// <summary>
    /// 利楚商务扫呗支付扫码支付
    /// </summary>
    public class LcswPayPrepayHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        public LcswPayPrepayHandler(ILogger<LcswPayPrepayHandler> log, ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
        }
        protected override string contentFormat => "payType|merchantNo|terminalId|accessToken|terminalTrace|terminalTime|totalFee|subAppid|orderBody|attach";
        protected override int[] contentEncryptedIndexs => new int[] { 1 };
        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                var payType = infos[0];
                var merchantNo = infos[1];
                var terminalId = infos[2];
                var accessToken = infos[3];
                var terminalTrace = infos[4];
                var terminalTime = infos[5];
                var totalFee = infos[6];
                var subAppid = infos[7];
                var orderBody = infos[8];
                var attach = infos[9];
                //调用扫码支付接口
                var request = new LcswPayPrepayRequest
                {
                    PayType = payType,
                    ServiceId = "011",
                    MerchantNo = merchantNo,
                    TerminalId = terminalId,
                    TerminalTime = terminalTime,
                    TerminalTrace = terminalTrace,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(totalFee) * 100).ToString(),
                    SubAppid = subAppid,
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
                    return HandleResult.Fail($"错误代码{response.ResultCode};错误描述:{response.ReturnMsg}");
                }
                var resultStr = $"{response.OutTradeNo}|{response.QrCode}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
