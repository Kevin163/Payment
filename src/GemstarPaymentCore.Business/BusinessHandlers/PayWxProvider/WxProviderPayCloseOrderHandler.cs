using System;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider
{

    /// <summary>
    /// 微信服务商关单
    /// 特别注意：如果收款已经成功，调用此接口仍然将关单成功，并且将收到的款项退回去
    /// </summary>
    public class WxProviderPayCloseOrderHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _options;
        public WxProviderPayCloseOrderHandler(ILogger<WxProviderPayCloseOrderHandler> log, IWeChatPayClient client, IOptionsSnapshot<WeChatPayOptions> options)
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
                int i = 0;
                var subAppid = infos[i++];
                var subMchId = infos[i++];
                var outTradeNo = infos[i++];

                var request = new WeChatPayCloseOrderRequest
                {
                    OutTradeNo = outTradeNo,
                    SubMchId = subMchId,
                    SubAppId = subAppid
                };

                var response = await _client.ExecuteAsync(request);

                if (response.ReturnCode != "SUCCESS")
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "SUCCESS")
                {
                    return HandleResult.Fail($"错误代码{response.ErrCode};错误描述:{response.ErrCodeDes}");
                }
                return HandleResult.Success("");
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
