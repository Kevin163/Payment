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
    /// 利楚商务扫呗预授权二维码生成，根据参数中的微信或者支付宝生成对应的二维码
    /// 如果想使用聚合二维码，请使用另外的接口
    /// </summary>
    public class LcswPayPreAuthQrcodeHandler: BusinessHandlerBase
    {
        private ILogger _log;
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        private readonly BusinessOption _businessOption;
        private string _businessContent;
        public LcswPayPreAuthQrcodeHandler(ILogger<LcswPayBarcodePayHandler> log, ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options,IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = log;
            _client = client;
            _options = options.Value;
            _businessOption = businessOption.Value;
        }
        protected override string contentFormat => "payType|merchantNo|terminalId|accessToken|terminalTrace|terminalTime|totalFee|orderBody|attach";
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
                var totalFee = infos[i++];
                var orderBody = infos[i++];
                var attach = infos[i++];
                //调用条码支付接口
                var request = new LcswPayPreAuthQrcodeRequest
                {
                    PayType = payType,
                    MerchantNo = merchantNo,
                    TerminalId = terminalId,
                    TerminalTime = terminalTime,
                    TerminalTrace = terminalTrace,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(totalFee) * 100).ToString(),
                    //目前如果传递此字段的话，扫呗会直接报签名错误导致无法支付，等扫呗修复此问题后，再考虑放开此参数的传递,先将orderbody传到attach参数中
                    //OrderBody = orderBody,
                    //Attach = attach
                    Attach = orderBody
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
                var resultStr = $"{response.QrcodeUrl}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
