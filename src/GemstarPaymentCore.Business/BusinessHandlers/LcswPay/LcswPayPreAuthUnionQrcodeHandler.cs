using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.LcswPay.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GemstarPaymentCore.Business.BusinessHandlers.LcswPay
{
    /// <summary>
    /// 利楚商务扫呗预授权聚合二维码
    /// </summary>
    public class LcswPayPreAuthUnionQrcodeHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "merchantNo|terminalId|accessToken|terminalTrace|terminalTime|totalFee|orderBody|attach";
        private const char splitChar = '|';
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        private readonly BusinessOption _businessOption;
        private string _businessContent;
        public LcswPayPreAuthUnionQrcodeHandler(ILogger<LcswPayBarcodePayHandler> log, ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options, IOptionsSnapshot<BusinessOption> businessOption)
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
                var merchantNo = infos[i++];
                var terminalId = infos[i++];
                var accessToken = infos[i++];
                var terminalTrace = infos[i++];
                var terminalTime = infos[i++];
                var totalFee = infos[i++];
                var orderBody = infos[i++];
                var attach = infos[i++];
                //判断参数有效性
                if (string.IsNullOrEmpty(merchantNo))
                {
                    return HandleResult.Fail("缺少商户号");
                }
                if (string.IsNullOrEmpty(terminalId))
                {
                    return HandleResult.Fail("缺少设备id");
                }
                if (string.IsNullOrEmpty(accessToken))
                {
                    return HandleResult.Fail("缺少访问令牌");
                }
                if (string.IsNullOrEmpty(terminalTrace))
                {
                    return HandleResult.Fail("缺少交易流水号");
                }
                if (string.IsNullOrEmpty(terminalTime))
                {
                    return HandleResult.Fail("缺少交易时间");
                }
                if (string.IsNullOrEmpty(totalFee))
                {
                    return HandleResult.Fail("缺少金额");
                }
                if (string.IsNullOrEmpty(orderBody))
                {
                    return HandleResult.Fail("缺少订单描述");
                }
                //构建接口地址，整个地址直接做为二维码内容返回
                var paras = new List<LcswPayParaInfo>
                {
                    new LcswPayParaInfo("merchant_no",merchantNo),
                    new LcswPayParaInfo("terminal_id",terminalId),
                    new LcswPayParaInfo("terminal_trace",terminalTrace),
                    new LcswPayParaInfo("terminal_time",terminalTime),
                    new LcswPayParaInfo("total_fee",Convert.ToInt32(Convert.ToDecimal(totalFee) * 100).ToString()),
                    new LcswPayParaInfo("order_body",orderBody),
                    new LcswPayParaInfo("auto_pay","1"),
                    new LcswPayParaInfo("attach",attach)
                };
                //计算签名
                var sign = LcswPaySignature.CalcSignWithAllNotNullParaAndToken(paras, accessToken);
                paras.Add(new LcswPayParaInfo("key_sign", sign));
                //组织完整地址做为二维码内容返回
                var urlStrBuilder = new StringBuilder();
                urlStrBuilder.Append(_options.ApiBaseUrl).Append("/pos/200/preauth/preAuthQrH5pay");
                var split = "?";
                foreach(var para in paras)
                {
                    if (para.ParaValue != null)
                    {
                        urlStrBuilder.Append(split).Append(para.ParaName).Append("=").Append(para.ParaValue);
                        split = "&";
                    }
                }

                return HandleResult.Success(urlStrBuilder.ToString());
            }
            catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }

    }
}
