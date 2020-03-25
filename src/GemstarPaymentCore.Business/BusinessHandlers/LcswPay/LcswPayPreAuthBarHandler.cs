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
    /// 利楚商务条码预授权处理
    /// </summary>
    public class LcswPayPreAuthBarHandler: BusinessHandlerBase
    {
        private ILogger _log;
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        private readonly BusinessOption _businessOption;
        private string _businessContent;
        public LcswPayPreAuthBarHandler(ILogger<LcswPayBarcodePayHandler> log, ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options,IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = log;
            _client = client;
            _options = options.Value;
            _businessOption = businessOption.Value;
        }
        protected override string contentFormat => "merchantNo|terminalId|accessToken|terminalTrace|terminalTime|authNo|totalFee|orderBody|attach";
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
                var authNo = infos[i++];
                var totalFee = infos[i++];
                var orderBody = infos[i++];
                var attach = infos[i++];
                //调用条码支付接口
                var request = new LcswPayPreAuthBarRequest
                {
                    MerchantNo = merchantNo,
                    TerminalId = terminalId,
                    TerminalTime = terminalTime,
                    TerminalTrace = terminalTrace,
                    AuthNo = authNo,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(totalFee) * 100).ToString(),
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
                        LcswPayPreAuthOrderQueryResponse queryResponse = null;
                        while (DateTime.Now < endDate)
                        {
                            if (DateTime.Now < queryDate)
                            {
                                continue;
                            }
                            queryDate = DateTime.Now.AddSeconds(2);
                            var queryRequest = new LcswPayPreAuthOrderQueryRequest
                            {
                                PayType = response.PayType,
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

                            if (queryResponse.IsReturnCodeSuccess && queryResponse.TradeState == "SUCCESS" && queryResponse.PayStatusCode == "7")
                            {
                                //已经支付成功
                                var queryResultStr = $"{queryResponse.OutTradeNo}|{response.PayType}|{queryResponse.ChannelTradeNo}|{queryResponse.MerchantName}|{queryResponse.EndTime}|{queryResponse.Attach}";
                                return HandleResult.Success(queryResultStr);
                            }
                        }
                        if (queryResponse != null)
                        {
                            return HandleResult.Fail($"错误代码{queryResponse.ResultCode};错误描述:{queryResponse.ReturnMsg}|{queryResponse.PayStatusCode}");
                        }
                    }
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
