using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider
{
    /// <summary>
    /// 酒店押金支付结果查询
    /// </summary>
    public class WxProviderDepositOrderQueryHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _options;
        public WxProviderDepositOrderQueryHandler(ILogger<WxProviderDepositOrderQueryHandler> log, IWeChatPayClient client, IOptionsSnapshot<WeChatPayOptions> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
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
                //开始查询
                var wxDepositOrderQuery = new WeChatPayDepositOrderQueryRequest
                {
                    SubAppId = subAppId,
                    SubMchId = subMchId,
                    OutTradeNo = outTradeNo
                };
                var response = await _client.ExecuteAsync(wxDepositOrderQuery);



                if (response.ReturnCode != "SUCCESS")
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "SUCCESS")
                {
                    return HandleResult.Fail($"错误代码{response.ErrCode};错误描述:{response.ErrCodeDes}");
                }
                if(response.TradeState != "SUCCESS")
                {
                    return HandleResult.Fail($"支付状态{response.TradeState};支付状态说明{response.TradeStateDesc};错误代码{response.ErrCode};错误描述:{response.ErrCodeDes}");
                }
                var resultStr = $"{response.TradeState}|{response.TradeStateDesc}|{response.TransactionId}|{response.TimeEnd}|{response.TotalFee}|{response.ConsumeFee}|{response.Attach}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }


    }
}
