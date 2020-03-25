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
    /// 支付宝预授权发码
    /// </summary>
    public class AlipayAuthQrcodeHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        public AlipayAuthQrcodeHandler(ILogger<AlipayAuthQrcodeHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
        }
        protected override string contentFormat => "orderNo|requestNo|orderAmount|content|payTimeOut|AppId|PId";
        protected override int[] contentEncryptedIndexs => new int[] { 5, 6 };

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
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
                var model = new AlipayFundAuthOrderVoucherCreateModel
                {
                    OutOrderNo = orderNo,
                    OutRequestNo = requestNo,
                    OrderTitle = content,
                    Amount = orderAmount,
                    PayeeUserId = _options.PId,
                    PayTimeout = payTimeOut,
                    ProductCode = "PRE_AUTH",
                    //ExtraParam = JsonConvert.SerializeObject(extraParam)
                };
                var request = new AlipayFundAuthOrderVoucherCreateRequest();
                request.SetBizModel(model);

                var response = await _client.ExecuteAsync(request, _options);
                var result = response.FailResult();
                if (response.IsSuccessCode())
                {
                    //var out_order_no = response.OutOrderNo;
                    //var out_request_no = response.OutRequestNo;
                    //var codeType = response.CodeType;
                    var codeValue = response.CodeValue;
                    //var codeUrl = response.CodeUrl;
                    //返回格式：(二维码内容)
                    //返回格式：(codeValue|out_order_no|operation_id|out_request_no|amount|payer_user_id|payer_logon_id)

                    var resultStr = codeValue;
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
