using System;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.Alipay;
using Essensoft.AspNetCore.Payment.Alipay.Domain;
using Essensoft.AspNetCore.Payment.Alipay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GemstarPaymentCore.Business.BusinessHandlers.Alipay
{
    /// <summary>
    /// 支付宝预授权冻结
    /// </summary>
    public class AlipayAuthFreezeHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "authCode|orderNo|requestNo|orderAmount|content|payTimeOut|AppId|PId";
        private const char splitChar = '|';
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        private readonly BusinessOption _businessOption;
        private string _businessContent;
        public AlipayAuthFreezeHandler(ILogger<AlipayAuthFreezeHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options,IOptionsSnapshot<BusinessOption> businessOption)
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
                var orderNo = infos[i++];
                var requestNo = infos[i++];
                var orderAmount = infos[i++];
                var content = infos[i++];
                var payTimeOut = infos[i++];
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
                    _options.RsaPublicKey = infos[i++];
                }
                if (i < infos.Length)
                {
                    _options.RsaPrivateKey = infos[i++];
                }
                if (i < infos.Length)
                {
                    _options.SignType = infos[i++];
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
                if (string.IsNullOrWhiteSpace(payTimeOut))
                {
                    payTimeOut = "15d";
                }

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
                var extraParam = new { category = "HOTEL" };
                var model = new AlipayFundAuthOrderFreezeModel
                {
                    AuthCode = authCode,
                    AuthCodeType = "bar_code",
                    OutOrderNo = orderNo,
                    OutRequestNo = requestNo,
                    OrderTitle = content,
                    Amount = orderAmount,
                    PayeeUserId = _options.PId,
                    PayTimeout = payTimeOut,
                    ProductCode = "PRE_AUTH",
                    ExtraParam = JsonConvert.SerializeObject(extraParam)
                };
                var request = new AlipayFundAuthOrderFreezeRequest();
                request.SetBizModel(model);

                var response =await _client.ExecuteAsync(request,_options);

                var result = response.FailResult();
                if (response.IsSuccessCode())
                {
                    var auth_no = response.AuthNo;
                    var out_order_no = response.OutOrderNo;
                    var operation_id = response.OperationId;
                    var out_request_no = response.OutRequestNo;
                    var amount = response.Amount;
                    var status = response.Status;
                    var payer_user_id = response.PayerUserId;
                    var payer_logon_id = response.PayerLogonId;
                    //如果状态是成功则返回相应信息
                    if (status == "SUCCESS")
                    {
                        //返回格式：(支付宝的资金授权订单号|商户的授权资金订单号|支付宝的资金操作流水号|商户本次资金操作的请求流水号|本次操作冻结的金额，单位为：元（人民币），精确到小数点后两位|付款方支付宝用户号|	收款方支付宝账号（Email或手机号）)
                        //返回格式：(auth_no|out_order_no|operation_id|out_request_no|amount|payer_user_id|payer_logon_id)

                        var resultStr = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", auth_no, out_order_no, operation_id, out_request_no, amount, payer_user_id, payer_logon_id);
                        result = HandleResult.Success(resultStr);
                        return result;
                    }
                }
                //如果是失败，则需要再次查询一次
                var timeout = _businessOption.BarcodePayTimeout;
                var endDate = DateTime.Now.AddSeconds(timeout);
                var queryDate = DateTime.Now.AddSeconds(2);
                var queryModel = new AlipayFundAuthOperationDetailQueryModel
                {
                    OutOrderNo = orderNo,
                    OutRequestNo = requestNo
                };
                var queryRequest = new AlipayFundAuthOperationDetailQueryRequest();
                queryRequest.SetBizModel(queryModel);
                while (DateTime.Now < endDate)
                {
                    if (DateTime.Now < queryDate)
                    {
                        continue;
                    }
                    queryDate = DateTime.Now.AddSeconds(2);
                    var queryResponse = await _client.ExecuteAsync(queryRequest,_options);
                    if (queryResponse.IsSuccessCode())
                    {
                        var auth_no = queryResponse.AuthNo;
                        var out_order_no = queryResponse.OutOrderNo;
                        var operation_id = queryResponse.OperationId;
                        var out_request_no = queryResponse.OutRequestNo;
                        var amount = queryResponse.Amount;
                        var status = queryResponse.Status;
                        var payer_user_id = queryResponse.PayerUserId;
                        var payer_logon_id = queryResponse.PayerLogonId;
                        //如果状态是成功则返回相应信息
                        if (status == "SUCCESS")
                        {
                            //返回格式：(支付宝的资金授权订单号|商户的授权资金订单号|支付宝的资金操作流水号|商户本次资金操作的请求流水号|本次操作冻结的金额，单位为：元（人民币），精确到小数点后两位|付款方支付宝用户号|	收款方支付宝账号（Email或手机号）)
                            //返回格式：(auth_no|out_order_no|operation_id|out_request_no|amount|payer_user_id|payer_logon_id)

                            var resultStr = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", auth_no, out_order_no, operation_id, out_request_no, amount, payer_user_id, payer_logon_id);
                            result = HandleResult.Success(resultStr);
                            return result;
                        } else if (status == "CLOSED")
                        {
                            return HandleResult.Fail("授权已经关闭");
                        }
                    } else if (queryResponse.Code == ResultCode.ServiceUnavailable || queryResponse.Code == ResultCode.WaitingUser)
                    {
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
