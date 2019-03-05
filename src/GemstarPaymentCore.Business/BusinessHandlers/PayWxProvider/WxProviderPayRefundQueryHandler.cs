﻿using System;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider
{

    /// <summary>
    /// 微信服务商退款查询
    /// </summary>
    public class WxProviderPayRefundQueryHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "subAppId|subMchId|outRefundNo";
        private const char splitChar = '|';
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _options;
        private string _businessContent;
        public WxProviderPayRefundQueryHandler(ILogger<WxProviderPayRefundQueryHandler> log, IWeChatPayClient client, IOptionsSnapshot<WeChatPayOptions> options)
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
                int i = 0;
                var subAppId = infos[i++];
                var subMchId = infos[i++];
                var outRefundNo = infos[i++];

                var request = new WeChatPayRefundQueryRequest
                {
                    OutRefundNo = outRefundNo,
                    SubAppId = subAppId,
                    SubMchId = subMchId
                };

                var response = await _client.ExecuteAsync(request);
                //根据返回消息进行处理
                if (response.ReturnCode == "SUCCESS")
                {
                    //通信成功，则判断业务结果
                    if (response.ResultCode == "SUCCESS" && response.RefundInfos.Count > 0)
                    {
                        var refundInfo = response.RefundInfos[0];
                        var status = refundInfo.RefundStatus;
                        var totalFeeByQuery = (refundInfo.RefundFee / 100.0).ToString("0.00");

                        if (refundInfo.SettlementRefundFee > 0)
                        {
                            totalFeeByQuery = (refundInfo.SettlementRefundFee / 100.0).ToString("0.00");
                        }
                        var transactionIdByQuery = refundInfo.RefundId;
                        var timeEndByQuery = DateTime.Now.ToString("yyyyMMddHHmmss");
                        //0refundId微信退款单号|退款时间20141030133525|退款金额
                        var resultStrByQuery = $"{transactionIdByQuery}|{timeEndByQuery}|{totalFeeByQuery}|{status}";
                        return HandleResult.Success(resultStrByQuery);
                    } else
                    {
                        //失败，记录失败原因到日志里面
                        var _logStr = $"查询微信服务商退款时遇到业务错误,代码:{response.ErrCode},描述:{response.ErrCodeDes}";
                        _log.LogError("WxProviderPayRefundQueryHandler", _logStr);
                        return HandleResult.Fail(_logStr);
                    }
                } else
                {
                    //通信失败，直接记录失败原因到日志里面
                    var _logStr = $"查询微信服务商退款时遇到通信错误:{response.ReturnMsg}";
                    _log.LogError("WxProviderPayRefundQueryHandler", _logStr);
                    return HandleResult.Fail(_logStr);
                }
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}