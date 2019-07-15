using GemstarPaymentCore.Business.BusinessQuery;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GemstarPaymentCore.Data;

namespace GemstarPaymentCore.Business.BusinessHandlers.Gemstar
{
    /// <summary>
    /// 捷信达聚合支付结果查询
    /// </summary>
    public class JxdUnionLcswPayQueryHandler : IBusinessHandler
    {
        private const string contentFormat = "terminalTrace|systemName";
        private const char splitChar = '|';
        private readonly IHttpClientFactory _clientFactory;
        private BusinessHandlerParameter _para;
        private BusinessOption _businessOption;
        private string _businessContent;
        public JxdUnionLcswPayQueryHandler(IHttpClientFactory client,IOptionsSnapshot<BusinessOption> businessOption)
        {
            _clientFactory = client;
            _businessOption = businessOption.Value;
        }


        public void SetBusinessContent(string businessContent)
        {
            _businessContent = businessContent;
        }
        public void SetBusiessHandlerParameter(BusinessHandlerParameter para)
        {
            _para = para;
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

                        var connStr = _businessOption.Systems.First(w => w.Name == systemName).ConnStr;
                        var dbContextOption = new DbContextOptionsBuilder<WxPayDB>();
                        dbContextOption.UseSqlServer(connStr);
                        using (var payDB = new WxPayDB(dbContextOption.Options))
                        {

                            WxPayInfoHelper.JxdUnionPayPaidSuccess(payDB, terminalTrace, transactionId, paidTime, paidAmount, payResult.PaidType);
                        }
                        return HandleResult.Success($"{transactionId}|{paidTime}|{paidAmount}");
                    }
                    else
                    {
                        return HandleResult.Fail(payResult.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }


    }
}
