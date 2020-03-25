using System;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider
{
    /// <summary>
    /// 微信服务商扫码支付的udp请求处理类，负责接收udp内容，调用统一下单后，返回二维码内容
    /// </summary>
    public class WxProviderPayQrcodePayHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _options;
        public WxProviderPayQrcodePayHandler(ILogger<WxProviderPayQrcodePayHandler> log,IWeChatPayClient client,IOptionsSnapshot<WeChatPayOptions> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
        }
        protected override string contentFormat => "subAppid|subMchId|body|outTradeNo|orderAmount";
        protected override int[] contentEncryptedIndexs => new int[] { 1 };

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                var subAppId = infos[0];
                var subMchId = infos[1];
                var body = infos[2];
                var outTradeNo = infos[3];
                var orderAmount = infos[4];
                //开始下单
                var wxQrcodeRequest = new WeChatPayUnifiedOrderRequest
                {
                    Body = body,
                    OutTradeNo = outTradeNo,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(orderAmount) * 100),
                    TradeType = "NATIVE",
                    ProductId = outTradeNo,
                    SubMchId = subMchId,
                    SubAppId = subAppId,
                    NotifyUrl = _options.NotifyUrl,
                    SpbillCreateIp = _options.CreateIP
                };
                var response = await _client.ExecuteAsync(wxQrcodeRequest);



                if (response.ReturnCode != "SUCCESS")
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "SUCCESS")
                {
                    return HandleResult.Fail($"错误代码{response.ErrCode};错误描述:{response.ErrCodeDes}");
                }
                var qrcodeUrl = response.CodeUrl;
                return HandleResult.Success(qrcodeUrl);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
