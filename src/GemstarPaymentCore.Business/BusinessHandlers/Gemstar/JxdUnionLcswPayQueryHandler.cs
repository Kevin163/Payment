using GemstarPaymentCore.Business.BusinessQuery;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.Gemstar
{
    /// <summary>
    /// 捷信达聚合支付结果查询
    /// </summary>
    public class JxdUnionLcswPayQueryHandler : BusinessHandlerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private BusinessHandlerParameter _para;
        private BusinessOption _businessOption;
        public JxdUnionLcswPayQueryHandler(IHttpClientFactory client,IOptionsSnapshot<BusinessOption> businessOption)
        {
            _clientFactory = client;
            _businessOption = businessOption.Value;
        }
        protected override string contentFormat => "terminalTrace|systemName";
        protected override int[] contentEncryptedIndexs => new int[] { };
        public void SetBusiessHandlerParameter(BusinessHandlerParameter para)
        {
            _para = para;
        }
        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                var terminalTrace = infos[i++];
                var systemName = infos[i++];
                //检查参数有效性     
                if (string.IsNullOrEmpty(terminalTrace))
                {
                    return HandleResult.Fail("必须传递terminalTrace参数");
                }
                //计算捷信达线上的查询地址
                var uriBase = new Uri(_businessOption.JxdPaymentUrl);
                var queryPath = "/JxdUnionPay/QueryPayResult";
                var queryUri = new Uri(uriBase, queryPath).AbsoluteUri;
                var httpClient = _clientFactory.CreateClient();
                using (var requestContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>> {
                                        KeyValuePair.Create("terminalTrace",terminalTrace)
                                    }))
                using (var response = await httpClient.PostAsync(queryUri, requestContent))
                using (var responseContent = response.Content)
                {
                    var resultStr = await responseContent.ReadAsStringAsync();
                    var payResult = JsonConvert.DeserializeObject<JxdUnionPayResult>(resultStr);
                    if (payResult.PaySuccess)
                    {
                        //支付成功
                        var paidTime = payResult.PaidTime;
                        var transactionId = payResult.PaidTransId;
                        var paidAmount = payResult.PaidAmount;
                        return HandleResult.Success($"{transactionId}|{paidTime}|{paidAmount}|{payResult.PaidType}|{terminalTrace}");
                    } else
                    {
                        return HandleResult.Fail(payResult.ErrorMessage);
                    }
                }
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
