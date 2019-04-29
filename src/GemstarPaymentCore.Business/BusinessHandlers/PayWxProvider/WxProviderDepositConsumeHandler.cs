using Essensoft.AspNetCore.Payment.WeChatPay;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Essensoft.AspNetCore.Payment.WeChatPay.Response;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider
{
    /// <summary>
    /// 酒店押金支付消费接口
    /// </summary>
    public class WxProviderDepositConsumeHandler : IBusinessHandler
    {
        private const string contentFormat = "subAppid|subMchId|outTradeNo|totalFee|consumeFee";
        private const char splitChar = '|';
        private readonly IWeChatPayClient _client;
        private readonly BusinessOption _options;
        private string _businessContent;
        public WxProviderDepositConsumeHandler(IWeChatPayClient client, IOptionsSnapshot<BusinessOption> options)
        {
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
                var outTradeNo = infos[2];
                var totalFee = infos[3];
                var consumeFee = infos[4];
                //开始查询
                var wxDepositConsume = new WeChatPayDepositConsumeRequest
                {
                    SubAppId = subAppId,
                    SubMchId = subMchId,
                    OutTradeNo = outTradeNo,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(totalFee) * 100).ToString(),
                    ConsumeFee = Convert.ToInt32(Convert.ToDecimal(consumeFee) * 100).ToString(),
                    FeeType = "CNY"
                };
                var response = await _client.ExecuteAsync(wxDepositConsume, ConfigHelper.WxPayCertificateName);



                if (response.ReturnCode != "SUCCESS")
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "SUCCESS")
                {
                    //SETTLING 押金消费已受理 押金消费已受理，请稍后查询订单订单状态确认最终结果
                    if(response.ErrCode == "SETTLING")
                    {
                        WeChatPayDepositOrderQueryResponse queryResponse = null;
                        var timeout = _options.BarcodePayTimeout;
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
                            if (queryResponse.ReturnCode == "SUCCESS" && queryResponse.ResultCode == "SUCCESS")
                            {
                                var queryResultStr = $"{queryResponse.TransactionId}|{queryResponse.TotalFee}|{queryResponse.ConsumeFee}";
                                return HandleResult.Success(queryResultStr);
                            }
                        }
                        if (queryResponse != null)
                        {
                            return HandleResult.Fail($"错误代码{queryResponse.ErrCode};错误描述:{queryResponse.ErrCodeDes}");
                        }
                       
                    }
                    return HandleResult.Fail($"错误代码{response.ErrCode};错误描述:{response.ErrCodeDes}");
                }
                var resultStr = $"{response.TransactionId}|{response.TotalFee}|{response.ConsumeFee}";
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }


    }
}
