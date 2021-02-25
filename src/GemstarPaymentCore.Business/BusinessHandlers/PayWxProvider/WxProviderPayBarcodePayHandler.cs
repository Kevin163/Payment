using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider
{
    /// <summary>
    /// 微信服务商条码支付的udp请求处理类，负责接收udp内容，调用条码支付类进行支付，并且返回相应结果
    /// </summary>
    public class WxProviderPayBarcodePayHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _options;
        private readonly BusinessOption _businessOption;
        public WxProviderPayBarcodePayHandler(ILogger<WxProviderPayBarcodePayHandler> log, IWeChatPayClient client, IOptionsSnapshot<WeChatPayOptions> options,IOptionsSnapshot<BusinessOption> businessOptions)
        {
            _log = log;
            _client = client;
            _options = options.Value;
            _businessOption = businessOptions.Value;
        }
        protected override string contentFormat => "subAppid|subMchId|body|outTradeNo|orderAmount|authCode";
        protected override int[] contentEncryptedIndexs => new int[] { 1 };

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int index = 0;
                var subAppid = infos[index++];
                var subMchId = infos[index++];
                var body = infos[index++];
                var outTradeNo = infos[index++];
                var orderAmount = infos[index++];
                var authCode = infos[index++];
                //获取新增加的订单优惠标记参数，为保持兼容性，此参数没有直接放到要求的内容格式中
                var goodsTag = GetParaValueSafely(infos, index++, "");
#if MOCK
                //如果定义了模拟编译变量，则直接根据金额来返回一个固定的结果，金额小于5则返回失败，金额大于等于5则直接返回支付成功
                if(Convert.ToDecimal(orderAmount) < 5){
                    return HandleResult.Fail("Mock测试时，金额小于5的会直接返回失败信息，请使用金额大于5的来测试支付成功结果");
                }else{
                    return HandleResult.Success("mocktransno|19000101000000|"+orderAmount);
                }
#endif
                //开始下单
                var request = new WeChatPayMicroPayRequest
                {
                    AuthCode = authCode,
                    Body = body,
                    OutTradeNo = outTradeNo,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(orderAmount) * 100),
                    SubAppId = subAppid,
                    SubMchId = subMchId,
                    GoodsTag = goodsTag
                };
                var response = await _client.ExecuteAsync(request);

                if (response.ReturnCode != "SUCCESS")
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "SUCCESS")
                {
                    //如果错误码是支付结果未知状态的，则循环来获取订单状态，直到订单状态为确认状态的，再返回相应结果
                    var errCode = response.ErrCode;
                    if (errCode.Equals("SYSTEMERROR", StringComparison.OrdinalIgnoreCase) || errCode.Equals("BANKERROR", StringComparison.OrdinalIgnoreCase) || errCode.Equals("USERPAYING", StringComparison.OrdinalIgnoreCase))
                    {
                        _log.LogInformation("WxProviderPayBarcodePayHandler", string.Format("条码支付后返回结果未知的错误码:{0}，等待稍后订单查询到确切状态后再行返回", errCode));
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
                            var queryRequest = new WeChatPayOrderQueryRequest
                            {
                                SubAppId = subAppid,
                                SubMchId = subMchId,
                                OutTradeNo = outTradeNo
                            };
                            var queryResponse = await _client.ExecuteAsync(queryRequest);
                            //根据返回消息进行处理
                            if (queryResponse.ReturnCode == "SUCCESS")
                            {
                                //通信成功，则判断业务结果
                                if (queryResponse.ResultCode == "SUCCESS")
                                {
                                    //业务结果返回成功，判断支付状态
                                    var tradeState = queryResponse.TradeState;
                                    _log.LogInformation("WxProviderPayBarcodePayHandler", string.Format("查询到的订单支付状态:{0}", tradeState));
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

                                        var resultStrByQuery = $"{transactionIdByQuery}|{timeEndByQuery}|{(totalFeeByQuery/100.0).ToString("0.00")}";
                                        return HandleResult.Success(resultStrByQuery);
                                    }
                                    else if (tradeState == "USERPAYING" || tradeState == "NOTPAY" || tradeState == "SYSTEMERROR" || tradeState == "BANKERROR")
                                    {
                                        //用户支付中
                                        //NOTPAY是指打印出了二维码，但客人还没有扫描，但接下来客人有可能会继续扫的，所以等下继续查询状态
                                        //不再任何处理，等下再次查询即可
                                    }
                                    else
                                    {
                                        //支付失败
                                        return HandleResult.Fail($"错误代码{tradeState};错误描述:{queryResponse.TradeStateDesc}");
                                    }
                                }
                                else
                                {
                                    //业务查询失败，记录失败原因到日志里面
                                    _log.LogError("WxProviderPayBarcodePayHandler", $"查询微信服务商订单状态时遇到业务错误,代码:{queryResponse.ErrCode},描述:{queryResponse.ErrCodeDes}");
                                }
                            }
                            else
                            {
                                //通信失败，直接记录失败原因到日志里面
                                _log.LogError("WxProviderPayBarcodePayHandler", $"查询微信服务商订单状态时遇到通信错误:{queryResponse.ReturnMsg}");
                            }
                        }
                    }
                    return HandleResult.Fail($"错误代码{errCode};错误描述:{response.ErrCodeDes}");
                }
                var totalFee = response.TotalFee;
                if (!string.IsNullOrWhiteSpace(response.SettlementTotalFee))
                {
                    totalFee = response.SettlementTotalFee;
                }
                totalFee = (Convert.ToInt32(totalFee) / 100.0).ToString("0.00");
                var transactionId = response.TransactionId;
                var timeEnd = response.TimeEnd;
                var resultStr = $"{transactionId}|{timeEnd}|{totalFee}";
                return HandleResult.Success(resultStr);
            }
            catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }

    }
}
