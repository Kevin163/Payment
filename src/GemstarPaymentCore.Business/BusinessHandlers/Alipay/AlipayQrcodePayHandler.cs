﻿using System;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.Alipay;
using Essensoft.AspNetCore.Payment.Alipay.Domain;
using Essensoft.AspNetCore.Payment.Alipay.Request;
using GemstarPaymentCore.Business.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GemstarPaymentCore.Business.BusinessHandlers.Alipay
{
    /// <summary>
    /// 支付宝扫码支付的udp内容处理类，负责接收udp请求后调用扫码支付的实现，然后返回二维码内容给前端，以便生成二维码给客人扫码支付
    /// </summary>
    public class AlipayQrcodePayHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "orderNo|operatorName|orderAmount|content|AppId|PID";
        private const char splitChar = '|';
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        private string _businessContent;
        public AlipayQrcodePayHandler(ILogger<AlipayQrcodePayHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options)
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
                var orderNo = infos[i++];
                var operatorName = infos[i++];
                var orderAmount = infos[i++];
                var content = infos[i++];
                _options.AppId = infos.GetNotEmptyValue(i++, _options.AppId);
                _options.PId = infos.GetNotEmptyValue(i++, _options.PId);
                _options.RsaPublicKey = infos.GetNotEmptyValue(i++, _options.RsaPublicKey);
                _options.RsaPrivateKey = infos.GetNotEmptyValue(i++, _options.RsaPrivateKey);
                _options.SignType = infos.GetNotEmptyValue(i++, _options.SignType);
                if (string.IsNullOrEmpty(_options.AppId))
                {
                    return HandleResult.Fail("请指定支付宝收款账号信息");
                }
                if (string.IsNullOrEmpty(_options.RsaPublicKey) || string.IsNullOrEmpty(_options.RsaPrivateKey))
                {
                    return HandleResult.Fail("请指定支付宝对应的密钥信息");
                }

                orderAmount = Convert.ToDouble(orderAmount).ToString("0.00");

                var model = new AlipayTradePrecreateModel
                {
                    OutTradeNo = orderNo,
                    Subject = content,
                    TotalAmount = orderAmount,
                    Body = $"支付宝验证码：{_options.PId}",
                    OperatorId = operatorName,
                    ExtendParams = new ExtendParams
                    {
                        SysServiceProviderId = string.IsNullOrEmpty(_options.SysServiceProviderId) ? "2088221616228734":_options.SysServiceProviderId
                    }
                };
                var req = new AlipayTradePrecreateRequest();
                req.SetBizModel(model);
                req.SetNotifyUrl(_options.NotifyUrl);

                var response = await _client.ExecuteAsync(req,_options);
                if (response.IsSuccessCode())
                {
                    return HandleResult.Success(response.QrCode);
                }
                return response.FailResult();
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
