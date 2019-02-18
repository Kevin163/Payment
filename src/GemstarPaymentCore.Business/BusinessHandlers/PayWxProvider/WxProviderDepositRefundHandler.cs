using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider
{
    /// <summary>
    /// 酒店押金退款接口
    /// </summary>
    public class WxProviderDepositRefundHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "subAppId|subMchId|transactionId|outRefundNo|totalFee|refundFee|refundDesc";
        private const char splitChar = '|';
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _options;
        private string _businessContent;
        public WxProviderDepositRefundHandler(ILogger<WxProviderDepositRefundHandler> log, IWeChatPayClient client, IOptionsSnapshot<WeChatPayOptions> options)
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
                var transactionId = infos[2];
                var outRefundNo = infos[3];
                var totalFee = infos[4];
                var refundFee = infos[5];
                var refundDesc = infos[6];
                //开始查询
                var wxDepositConsume = new WeChatPayDepositRefundRequest
                {
                    SubAppId = subAppId,
                    SubMchId = subMchId,
                    TransactionId = transactionId,
                    OutRefundNo = outRefundNo,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(totalFee) * 100),
                    RefundFee = Convert.ToInt32(Convert.ToDecimal(refundFee) * 100),
                    RefundFeeType = "CNY",
                    RefundDesc = refundDesc
                };
                var response = await _client.ExecuteAsync(wxDepositConsume, ConfigHelper.WxPayCertificateName);



                if (response.ReturnCode != "SUCCESS")
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "SUCCESS")
                {
                    return HandleResult.Fail($"错误代码{response.ErrCode};错误描述:{response.ErrCodeDes}");
                }
                var resultStr = $"{response.RefundId}|{response.OutRefundNo}|{response.RefundFee}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }


    }
}
