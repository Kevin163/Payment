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
    /// 阿里预授权查询
    /// </summary>
    public class AlipayAuthQueryHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "outOrderNo|outRequestNo|Appid|Pid";
        private const char splitChar = '|';
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        private string _businessContent;
        public AlipayAuthQueryHandler(ILogger<AlipayAuthQueryHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options)
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
                var outOrderNo = infos[i++];
                var outRequestNo = infos[i++];
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

                var model = new AlipayFundAuthOperationDetailQueryModel
                {
                    OutOrderNo = outOrderNo,
                    OutRequestNo = outRequestNo
                };
                var request = new AlipayFundAuthOperationDetailQueryRequest();
                request.SetBizModel(model);

                var response = await _client.ExecuteAsync(request,_options);

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
                    } else if (status == "CLOSED")
                    {
                        return HandleResult.Fail("授权已经关闭");
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

