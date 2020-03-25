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
    /// 利楚商务扫呗支付条码支付
    /// </summary>
    public class LcswPayBarcodePayHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        private readonly BusinessOption _businessOption;
        private string _businessContent;
        public LcswPayBarcodePayHandler(ILogger<LcswPayBarcodePayHandler> log, ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options,IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = log;
            _client = client;
            _options = options.Value;
            _businessOption = businessOption.Value;
        }
        protected override string contentFormat => "payType|merchantNo|terminalId|accessToken|terminalTrace|terminalTime|authNo|totalFee|subAppid|orderBody|attach";
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
                var authNo = infos[i++];
                var totalFee = infos[i++];
                var subAppid = infos[i++];
                var orderBody = infos[i++];
                var attach = infos[i++];
                //调用条码支付接口
                var request = new LcswPayBarcodePayRequest
                {
                    PayType = payType,
                    ServiceId = "010",
                    MerchantNo = merchantNo,
                    TerminalId = terminalId,
                    TerminalTime = terminalTime,
                    TerminalTrace = terminalTrace,
                    AuthNo = authNo,
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
                    //如果是03支付中的话，则进行等待查询
                    if (response.ResultCode == "03")
                    {
                        var timeout = _businessOption.BarcodePayTimeout;
                        var endDate = DateTime.Now.AddSeconds(timeout);
                        var queryDate = DateTime.Now.AddSeconds(2);
                        LcswPayQueryResponse queryResponse = null;
                        while (DateTime.Now < endDate)
                        {
                            if (DateTime.Now < queryDate)
                            {
                                continue;
                            }
                            queryDate = DateTime.Now.AddSeconds(2);
                            var queryRequest = new LcswPayQueryRequest
                            {
                                PayType = payType,
                                ServiceId = "020",
                                MerchantNo = merchantNo,
                                TerminalId = terminalId,
                                TerminalTime = terminalTime,
                                TerminalTrace = terminalTrace,
                                PayTrace = request.TerminalTrace,
                                PayTime = request.TerminalTime,
                                OutTradeNo = response.OutTradeNo
                            };
                            _options.Token = accessToken;
                            queryResponse = await _client.ExecuteAsync(queryRequest, _options);

                            if (queryResponse.IsReturnCodeSuccess && queryResponse.TradeState == "SUCCESS")
                            {
                                //已经支付成功
                                var queryResultStr = $"{queryResponse.OutTradeNo}|{queryResponse.ChannelTradeNo}|{queryResponse.MerchantName}|{queryResponse.EndTime}|{queryResponse.Attach}|{queryResponse.PayType}";
                                return HandleResult.Success(queryResultStr);
                            }
                        }
                        if (queryResponse != null)
                        {
                            return HandleResult.Fail($"错误代码{queryResponse.ResultCode};错误描述:{queryResponse.ReturnMsg}");
                        }
                    }
                    return HandleResult.Fail($"错误代码{response.ResultCode};错误描述:{response.ReturnMsg}");
                }
                var resultStr = $"{response.OutTradeNo}|{response.ChannelTradeNo}|{response.MerchantName}|{response.EndTime}|{response.Attach}|{response.PayType}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
