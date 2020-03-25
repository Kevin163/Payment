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
    /// 支付宝预授权取消冻结
    /// </summary>
    public class AlipayAuthUnFreezeHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        public AlipayAuthUnFreezeHandler(ILogger<AlipayAuthUnFreezeHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
        }
        protected override string contentFormat => "authNo|outRequestNo|Amount|remark|AppId|PId";
        protected override int[] contentEncryptedIndexs => new int[] { 4, 5 };

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                var authNo = infos[i++];
                var outRequestNo = infos[i++];
                var Amount = infos[i++];
                var remark = infos[i++];
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

                Amount = Convert.ToDouble(Amount).ToString("0.00");

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
                var model = new AlipayFundAuthOrderUnfreezeModel
                {
                    AuthNo = authNo,
                    OutRequestNo = outRequestNo,
                    Amount = Amount,
                    Remark = remark
                };
                var request = new AlipayFundAuthOrderUnfreezeRequest();
                request.SetBizModel(model);

                var response = await _client.ExecuteAsync(request, _options);

                var result = response.FailResult();
                if (response.IsSuccessCode())
                {
                    var auth_no = response.AuthNo;
                    var out_order_no = response.OutOrderNo;
                    var operation_id = response.OperationId;
                    var out_request_no = response.OutRequestNo;
                    var amount = response.Amount;
                    var status = response.Status;
                    //如果状态是成功则返回相应信息
                    if (status == "SUCCESS")
                    {
                        //返回格式：(支付宝的资金授权订单号|商户的授权资金订单号|支付宝的资金操作流水号|商户本次资金操作的请求流水号|本次操作解冻的金额，单位为：元（人民币），精确到小数点后两位)
                        //返回格式：(auth_no|out_order_no|operation_id|out_request_no|amount)

                        var resultStr = string.Format("{0}|{1}|{2}|{3}|{4}", auth_no, out_order_no, operation_id, out_request_no, amount);
                        result = HandleResult.Success(resultStr);
                        return result;
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

