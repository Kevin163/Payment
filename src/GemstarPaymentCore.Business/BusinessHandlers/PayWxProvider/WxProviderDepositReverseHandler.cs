using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Essensoft.AspNetCore.Payment.WeChatPay.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider
{
    /// <summary>
    /// 酒店押金撤销订单
    /// </summary>
    public class WxProviderDepositReverseHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _options;
        private readonly BusinessOption _businessOption;
        public WxProviderDepositReverseHandler(ILogger<WxProviderDepositReverseHandler> log, IWeChatPayClient client, IOptionsSnapshot<WeChatPayOptions> options,IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = log;
            _client = client;
            _options = options.Value;
            _businessOption = businessOption.Value;
        }
        protected override string contentFormat => "subAppid|subMchId|outTradeNo";
        protected override int[] contentEncryptedIndexs => new int[] { 1 };
        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                var subAppId = infos[0];
                var subMchId = infos[1];
                var outTradeNo = infos[2];
                //开始撤销押金订单
                var wxDepositReverse = new WeChatPayDepositReverseRequest
                {
                    SubAppId = subAppId,
                    SubMchId = subMchId,
                    OutTradeNo = outTradeNo
                };
                var response = await _client.ExecuteAsync(wxDepositReverse,ConfigHelper.WxPayCertificateName);



                if (response.ReturnCode != "SUCCESS")
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "SUCCESS")
                {
                    //SYSTEMERROR 系统超时 请立即调用被扫订单结果查询API，查询当前订单状态，并根据订单的状态决定下一步的操作
                    if(response.ErrCode == "SYSTEMERROR")
                    {
                        WeChatPayDepositOrderQueryResponse queryResponse = null;
                        var timeout = _businessOption.BarcodePayTimeout;
                        var endDate = DateTime.Now.AddSeconds(timeout);
                        var queryDate = DateTime.Now.AddSeconds(2);
                        while (DateTime.Now < endDate)
                        {
                            if (DateTime.Now < queryDate)
                            {
                                continue;
                            }
                            queryDate = DateTime.Now.AddSeconds(2);
                            //开始查询
                            var queryRequest = new WeChatPayDepositOrderQueryRequest
                            {
                                SubAppId = subAppId,
                                SubMchId = subMchId,
                                OutTradeNo = outTradeNo
                            };
                            queryResponse = await _client.ExecuteAsync(queryRequest);
                            if (queryResponse.ReturnCode == "SUCCESS" && queryResponse.ResultCode == "SUCCESS" && queryResponse.TradeState == "REVOKED")
                            {
                                return HandleResult.Success("");
                            }
                        }
                        if(queryResponse != null)
                        {
                            return HandleResult.Fail($"错误代码{queryResponse.ErrCode};错误描述:{queryResponse.ErrCodeDes}");
                        }
                    }
                    return HandleResult.Fail($"错误代码{response.ErrCode};错误描述:{response.ErrCodeDes}");
                }
                var resultStr = "";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }


    }
}
