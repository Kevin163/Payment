using System;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider
{
    /// <summary>
    /// 微信服务商退款申请功能
    /// </summary>
    public class WxProviderPayRefundHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _options;
        public WxProviderPayRefundHandler(ILogger<WxProviderPayRefundHandler> log, IWeChatPayClient client, IOptionsSnapshot<WeChatPayOptions> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
        }
        protected override string contentFormat => "subAppId|subMchId|transactionId|outRefundNo|totalFee|refundFee|opUserId";
        protected override int[] contentEncryptedIndexs => new int[] { 1 };

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                var subAppId = infos[i++];
                var subMchId = infos[i++];
                var transactionId = infos[i++];
                var outRefundNo = infos[i++];
                var totalFee = infos[i++];
                var refundFee = infos[i++];
                var opUserId = infos[i++];

                var request = new WeChatPayRefundRequest
                {
                    OutRefundNo = outRefundNo,
                    TransactionId = transactionId,
                    SubAppId = subAppId,
                    SubMchId = subMchId,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(totalFee) * 100),
                    RefundFee = Convert.ToInt32(Convert.ToDecimal(refundFee) * 100),
                    
                };
#if MOCK
                //如果定义了模拟编译变量，则直接根据金额来返回一个固定的结果，金额小于5则返回失败，金额大于等于5则直接返回支付成功
                if (transactionId != "mocktransno")
                {
                    return HandleResult.Fail("Mock测试时，只能针对mocktransno交易号进行发起退款");
                }
                else
                {
                    return HandleResult.Success("mockrefundno|19000101000000|" + refundFee);
                }
#endif
                var response = await _client.ExecuteAsync(request,ConfigHelper.WxPayCertificateName);
                //根据返回消息进行处理
                if (response.ReturnCode == "SUCCESS")
                {
                    //通信成功，则判断业务结果
                    if (response.ResultCode == "SUCCESS")
                    {
                        //退款申请成功，具体退款状态需要另外查询，零钱的退款需要20分钟左右，其他的需要三个工作日
                        var totalFeeByQuery = response.RefundFee;

                        if (!string.IsNullOrWhiteSpace(response.SettlementRefundFee))
                        {
                            totalFeeByQuery = response.SettlementRefundFee;
                        }
                        var transactionIdByQuery = response.RefundId;
                        var timeEndByQuery = DateTime.Now.ToString("yyyyMMddHHmmss");
                        //0refundId微信退款单号|退款时间20141030133525|退款金额
                        var resultStrByQuery = $"{transactionIdByQuery}|{timeEndByQuery}|{(Convert.ToInt32(totalFeeByQuery) / 100.0).ToString("0.00")}";
                        return HandleResult.Success(resultStrByQuery);
                    } else
                    {
                        //失败，记录失败原因到日志里面
                        var _logStr = $"申请微信服务商退款时遇到业务错误,代码:{response.ErrCode},描述:{response.ErrCodeDes}";
                        _log.LogError("WxProviderRefundUdpContentHandler", _logStr);
                        return HandleResult.Fail(_logStr);
                    }
                } else
                {
                    //通信失败，直接记录失败原因到日志里面
                    var _logStr = $"申请微信服务商退款时遇到通信错误:{response.ReturnMsg}";
                    _log.LogError("WxProviderRefundUdpContentHandler", _logStr);
                    return HandleResult.Fail(_logStr);
                }
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
