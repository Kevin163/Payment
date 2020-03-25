using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.LcswPay
{
    /// <summary>
    /// 利楚商务扫呗聚合二维码支付接口
    /// </summary>
    public class LcswPayUnionQrcodePayHandler : BusinessHandlerBase
    {
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        public LcswPayUnionQrcodePayHandler(ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options)
        {
            _client = client;
            _options = options.Value;
        }
        protected override string contentFormat => "merchantNo|terminalId|accessToken|terminalTrace|terminalTime|totalFee|orderBody|attach";
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
                var totalFee = infos[i++];
                var orderBody = infos[i++];
                var attach = infos[i++];
                //调用扫码支付接口
                var request = new LcswPayUnionQrcodePayRequest
                {
                    MerchantNo = merchantNo,
                    TerminalId = terminalId,
                    TerminalTime = terminalTime,
                    TerminalTrace = terminalTrace,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(totalFee) * 100).ToString(),
                    OrderBody = orderBody,
                    Attach = attach,
                    NotifyUrl = ""
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
                var resultStr = $"{response.QrCode}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
