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
    /// 支付宝授权撤销
    /// </summary>
    public class AlipayAuthCancelHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "outOrderNo|outRequestNo|Remark|AppId|PId";
        private const char splitChar = '|';
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        private string _businessContent;
        public AlipayAuthCancelHandler(ILogger<AlipayAuthCancelHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options)
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
                var Remark = infos[i++];
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
                var model = new AlipayFundAuthOperationCancelModel
                {
                    OutOrderNo = outOrderNo,
                    OutRequestNo = outRequestNo,
                    Remark = Remark
                };
                var request = new AlipayFundAuthOperationCancelRequest();
                request.SetBizModel(model);

                var response = await _client.ExecuteAsync(request,_options);
                
                var result = response.FailResult();
                if (response.IsSuccessCode())
                {
                    var auth_no = response.AuthNo;
                    var out_order_no = response.OutOrderNo;
                    var operation_id = response.OperationId;
                    var out_request_no = response.OutRequestNo;
                    var action = response.Action;
                    var actionTitle = action;
                    if (action == "close")
                    {
                        actionTitle = "关闭冻结明细，无资金解冻 ";
                    } else if (action == "unfreeze")
                    {
                        actionTitle = "产生了资金解冻";
                    }

                    //返回格式：(支付宝的资金授权订单号|商户的授权资金订单号|支付宝的冻结操作流水号|商户的冻结操作流水号 |资金操作描述)
                    //返回格式：(auth_no|out_order_no|operation_id|out_request_no|actionTitle)

                    var resultStr = string.Format("{0}|{1}|{2}|{3}|{4}", auth_no, out_order_no, operation_id, out_request_no, actionTitle);
                    result = HandleResult.Success(resultStr);
                    return result;
                }
                return result;
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}

