using System;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.Alipay;
using Essensoft.AspNetCore.Payment.Alipay.Domain;
using Essensoft.AspNetCore.Payment.Alipay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GemstarPaymentCore.Business.BusinessHandlers.Alipay
{
    /// <summary>
    /// 支付宝预授权完成
    /// </summary>
    public class AlipayAuthFinishHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        private readonly BusinessOption _businessOption;
        protected override string contentFormat => "outTradeNo|totalAmount|authNo|subject|buyerId|SellerId|authConfirmMode|AppId";
        protected override int[] contentEncryptedIndexs => new int[] { 5,7 };
        public AlipayAuthFinishHandler(ILogger<AlipayAuthFinishHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options,IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = log;
            _client = client;
            _options = options.Value;
            _businessOption = businessOption.Value;
        }

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                var outTradeNo = infos[i++];
                var totalAmount = infos[i++];
                var authNo = infos[i++];
                var subject = infos[i++];
                var buyerId = infos[i++];
                var SellerId = infos[i++];
                var authConfirmMode = infos[i++];
                if (i < infos.Length)
                {
                    _options.AppId = infos[i++];
                }
                if (i < infos.Length)
                {
                    _options.PId = infos[i++];
                }
                if (i < infos.Length)
                {
                    var temp = infos[i++];
                    if (!string.IsNullOrEmpty(temp))
                    {
                        _options.RsaPublicKey = temp;
                    }
                }
                if (i < infos.Length)
                {
                    var temp = infos[i++];
                    if (!string.IsNullOrEmpty(temp))
                    {
                        _options.RsaPrivateKey = temp;
                    }
                }
                if (i < infos.Length)
                {
                    var temp = infos[i++];
                    if (!string.IsNullOrEmpty(temp))
                    {
                        _options.SignType = temp;
                    }
                }
                if (string.IsNullOrEmpty(_options.AppId))
                {
                    return HandleResult.Fail("请指定支付宝收款账号信息");
                }
                if (string.IsNullOrEmpty(_options.RsaPublicKey) || string.IsNullOrEmpty(_options.RsaPrivateKey))
                {
                    return HandleResult.Fail("请指定支付宝对应的密钥信息");
                }

                totalAmount = Convert.ToDouble(totalAmount).ToString("0.00");
#if MOCK
            //如果定义了模拟编译变量，则直接根据金额来返回一个固定的结果，金额小于5则返回失败，金额大于等于5则直接返回支付成功
            if (Convert.ToDecimal(builder.total_amount) < 5)
            {
                return HandleResult.Fail("Mock测试时，金额小于5的会直接返回失败信息，请使用金额大于5的来测试支付成功结果");
            }
            else
            {
                return HandleResult.Success("mocktransno|19000101000000|" + builder.total_amount);
            }
#endif

                var model = new AlipayTradePayModel
                {
                    OutTradeNo = outTradeNo,
                    TotalAmount = totalAmount,
                    ProductCode = "PRE_AUTH",
                    AuthNo = authNo,
                    Subject = subject,
                    BuyerId = buyerId,
                    SellerId = SellerId,
                    AuthConfirmMode = authConfirmMode,
                    ExtendParams = new ExtendParams
                    {
                        SysServiceProviderId = string.IsNullOrEmpty(_options.SysServiceProviderId) ? "2088221616228734" : _options.SysServiceProviderId
                    }
                };
                var request = new AlipayTradePayRequest();
                request.SetBizModel(model);

                var response = await _client.ExecuteAsync(request, _options);

                var result = response.FailResult();
                if (response.IsSuccessCode())
                {
                    var paidAmount = response.BuyerPayAmount;
                    var tradeNo = response.TradeNo;
                    var paidDate = DateTime.Now.ToString("yyyyMMddHHmmss");
                    if (!string.IsNullOrWhiteSpace(response.GmtPayment))
                    {
                        paidDate = DateTime.Parse(response.GmtPayment).ToString("yyyyMMddHHmmss");
                    }
                    var resultStr = string.Format("{0}|{1}|{2}", tradeNo, paidDate, paidAmount);
                    result = HandleResult.Success(resultStr);
                }
                //如果是失败，则需要再次查询一次
                var timeout = _businessOption.BarcodePayTimeout;
                var endDate = DateTime.Now.AddSeconds(timeout);
                var queryDate = DateTime.Now.AddSeconds(2);
                var queryModel = new AlipayTradeQueryModel
                {
                    OutTradeNo = outTradeNo
                };
                var queryRequest = new AlipayTradeQueryRequest();
                queryRequest.SetBizModel(queryModel);
                while (DateTime.Now < endDate)
                {
                    if (DateTime.Now < queryDate)
                    {
                        continue;
                    }
                    queryDate = DateTime.Now.AddSeconds(2);
                    var queryResponse = await _client.ExecuteAsync(queryRequest, _options);
                    if (queryResponse.IsSuccessCode())
                    {
                        if (queryResponse.TradeStatus == "TRADE_SUCCESS" || queryResponse.TradeStatus == "TRADE_FINISHED")
                        {
                            var paidAmount = queryResponse.ReceiptAmount;
                            var tradeNo = queryResponse.TradeNo;
                            var paidDate = DateTime.Parse(queryResponse.SendPayDate).ToString("yyyyMMddHHmmss");
                            var resultStr = string.Format("{0}|{1}|{2}", tradeNo, paidDate, paidAmount);
                            result = HandleResult.Success(resultStr);
                            return result;
                        } else if (queryResponse.TradeStatus == "TRADE_CLOSED")
                        {
                            return HandleResult.Fail("未付款交易超时关闭，或支付完成后全额退款");
                        }
                        result = queryResponse.FailResult();
                    } else if (queryResponse.Code == ResultCode.ServiceUnavailable || queryResponse.Code == ResultCode.WaitingUser)
                    {
                        result = queryResponse.FailResult();
                        continue;
                    } else
                    {
                        result = queryResponse.FailResult();
                        //中止查询
                        endDate = DateTime.Now.AddDays(-1);
                    }
                }
                return result;
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
