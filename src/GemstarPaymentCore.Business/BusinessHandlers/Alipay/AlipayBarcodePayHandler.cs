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
    /// 阿里支付宝条码支付的udp请求处理类，负责接收udp内容，调用条码支付类进行支付，并且返回相应结果
    /// </summary>
    public class AlipayBarcodePayHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "authCode|scene|orderNo|operatorName|orderAmount|content|AppId";
        private const char splitChar = '|';
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        private readonly BusinessOption _businessOption;
        private string _businessContent;
        public AlipayBarcodePayHandler(ILogger<AlipayBarcodePayHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options,IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = log;
            _client = client;
            _options = options.Value;
            _businessOption = businessOption.Value;
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
                var authCode = infos[i++];
                var scene = infos[i++];
                var orderNo = infos[i++];
                var operatorName = infos[i++];
                var orderAmount = infos[i++];
                var content = infos[i++];
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

                orderAmount = Convert.ToDouble(orderAmount).ToString("0.00");

                var model = new AlipayTradePayModel
                {
                    OutTradeNo = orderNo,
                    Scene = scene,
                    AuthCode = authCode,
                    TotalAmount = orderAmount,
                    OperatorId = operatorName,
                    Subject = content,
                    ExtendParams = new ExtendParams
                    {
                        SysServiceProviderId = string.IsNullOrEmpty(_options.SysServiceProviderId) ? "2088221616228734":_options.SysServiceProviderId
                    }
                };
                var request = new AlipayTradePayRequest();
                request.SetBizModel(model);

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
                var response = await _client.ExecuteAsync(request,_options);

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
                    return HandleResult.Success(resultStr);
                }
                var result = response.FailResult();
                //如果是失败，则需要再次查询一次
                var timeout = _businessOption.BarcodePayTimeout;
                var endDate = DateTime.Now.AddSeconds(timeout);
                var queryDate = DateTime.Now.AddSeconds(2);
                var queryModel = new AlipayTradeQueryModel
                {
                    OutTradeNo = orderNo
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
                    var queryResponse = await _client.ExecuteAsync(queryRequest);
                    _log.LogInformation("AlipayBarcodePayHandler", "查询结果详情：" + (queryResponse == null ? "queryResponse为null" : queryResponse.Body));
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
                        } else
                        {
                            result = queryResponse.FailResult();
                        }
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
                _log.LogInformation("AlipayBarcodePayHandler", "返回支付结果：" + result.ResultStr);
                return result;
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
