using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider
{
    /// <summary>
    /// 酒店押金支付冻结押金
    /// </summary>
    public class WxProviderDepositMicropayHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "subAppid|subMchId|body|outTradeNo|orderAmount|authCode|attach";
        private const char splitChar = '|';
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _options;
        private string _businessContent;
        public WxProviderDepositMicropayHandler(ILogger<WxProviderDepositMicropayHandler> log, IWeChatPayClient client,IOptionsSnapshot<WeChatPayOptions> options)
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
                var subAppId = infos[0];
                var subMchId = infos[1];
                var body = infos[2];
                var outTradeNo = infos[3];
                var orderAmount = infos[4];
                var authCode = infos[5];
                var attach = infos[6];
                //开始下单
                var wxDepositMicropay = new WeChatPayDepositMicroPayRequest
                {
                    SubAppId = subAppId,
                    SubMchId = subMchId,
                    Body = body,
                    OutTradeNo = outTradeNo,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(orderAmount) * 100),
                    AuthCode = authCode,
                    SpbillCreateIp = _options.CreateIP,
                    FeeType = "CNY",
                    Attach = attach
                };
                var response = await _client.ExecuteAsync(wxDepositMicropay);



                if (response.ReturnCode != "SUCCESS")
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "SUCCESS")
                {
                    //SYSTEMERROR 支付结果未知 请立即调用被扫订单结果查询API，查询当前订单状态，并根据订单的状态决定下一步的操作
                    //USERPAYING 支付结果未知 等待5秒，然后调用被扫订单结果查询API，查询当前订单的不同状态，决定下一步的操作
                    if(response.ErrCode == "USERPAYING" || response.ErrCode == "SYSTEMERROR")
                    {
                        //等待5秒
                        Thread.Sleep(5 * 1000);
                        //开始查询
                        var queryRequest = new WeChatPayDepositOrderQueryRequest
                        {
                            SubAppId = subAppId,
                            SubMchId = subMchId,
                            OutTradeNo = outTradeNo
                        };
                        var queryResponse = await _client.ExecuteAsync(queryRequest);
                        if (queryResponse.ReturnCode != "SUCCESS")
                        {
                            return HandleResult.Fail(queryResponse.ReturnMsg);
                        }
                        if (queryResponse.ResultCode != "SUCCESS")
                        {
                            return HandleResult.Fail($"错误代码{queryResponse.ErrCode};错误描述:{queryResponse.ErrCodeDes}");
                        }
                        if(queryResponse.TradeState != "SUCCESS")
                        {
                            return HandleResult.Fail($"支付状态{queryResponse.TradeState};支付状态说明{queryResponse.TradeStateDesc};错误代码{queryResponse.ErrCode};错误描述:{queryResponse.ErrCodeDes}");
                        }
                        var queryResultStr = $"{queryResponse.TransactionId}|{queryResponse.TimeEnd}|{queryResponse.TotalFee}|{queryResponse.Attach}";
                        return HandleResult.Success(queryResultStr);
                    }
                    return HandleResult.Fail($"错误代码{response.ErrCode};错误描述:{response.ErrCodeDes}");
                }
                var resultStr = $"{response.TransactionId}|{response.TimeEnd}|{response.TotalFee}|{response.Attach}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }


    }
}
