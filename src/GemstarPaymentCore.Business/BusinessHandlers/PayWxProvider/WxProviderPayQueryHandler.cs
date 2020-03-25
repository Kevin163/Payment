using System;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider
{
    /// <summary>
    /// 微信服务商支付状态查询功能，用于微信刷卡支付时，对于未知状态的进行查询确认
    /// </summary>
    public class WxProviderPayQueryHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _options;
        public WxProviderPayQueryHandler(ILogger<WxProviderPayQueryHandler> log, IWeChatPayClient client, IOptionsSnapshot<WeChatPayOptions> options)
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
                //开始查询
                var request = new WeChatPayOrderQueryRequest{
                    OutTradeNo = outTradeNo,
                    SubAppId = subAppid,
                    SubMchId = subMchId                    
                };
                var queryResponse = await _client.ExecuteAsync(request);
                //根据返回消息进行处理
                if (queryResponse.ReturnCode == "SUCCESS")
                {
                    //通信成功，则判断业务结果
                    if (queryResponse.ResultCode == "SUCCESS")
                    {
                        //业务结果返回成功，判断支付状态
                        var tradeState = queryResponse.TradeState;
                        _log.LogInformation("WxProviderPayQueryHandler", string.Format("查询到的订单支付状态:{0}", tradeState));
                        if (tradeState == "SUCCESS")
                        {
                            //支付成功
                            var totalFeeByQuery = queryResponse.TotalFee;
                            if (queryResponse.SettlementTotalFee > 0)
                            {
                                totalFeeByQuery = queryResponse.SettlementTotalFee;
                            }
                            var transactionIdByQuery = queryResponse.TransactionId;
                            var timeEndByQuery = queryResponse.TimeEnd;

                            var resultStrByQuery = $"{transactionIdByQuery}|{timeEndByQuery}|{(totalFeeByQuery / 100.0).ToString("0.00")}";
                            return HandleResult.Success(resultStrByQuery);
                        } else if (tradeState == "USERPAYING" || tradeState == "NOTPAY" || tradeState == "SYSTEMERROR" || tradeState == "BANKERROR")
                        {
                            //用户支付中
                            //NOTPAY是指打印出了二维码，但客人还没有扫描，但接下来客人有可能会继续扫的，所以等下继续查询状态
                            //不再任何处理，等下再次查询即可
                        } else
                        {
                            //支付失败
                            return HandleResult.Fail($"错误代码{tradeState};错误描述:{queryResponse.TradeStateDesc}");
                        }
                    } else
                    {
                        //业务查询失败，记录失败原因到日志里面
                        var msg = $"查询微信服务商订单状态时遇到业务错误,代码:{queryResponse.ErrCode},描述:{queryResponse.ErrCodeDes}";
                        _log.LogError("WxProviderPayQueryHandler", msg);
                        return HandleResult.Fail(msg);
                    }
                } else
                {
                    //通信失败，直接记录失败原因到日志里面
                    _log.LogError("WxProviderPayQueryHandler", $"查询微信服务商订单状态时遇到通信错误:{queryResponse.ReturnMsg}");
                }
                return HandleResult.Fail($"查询微信服务商订单状态时遇到通信错误:{queryResponse.ReturnMsg}");

            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
