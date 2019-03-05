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
    /// 阿里支付宝支付结果查询的udp请求处理类，负责接收udp内容，调用支付查询类进行查询，并且返回相应结果
    /// </summary>
    public class AlipayPayQueryHandler : IBusinessHandler
    {
        private ILogger _log;
        private const string contentFormat = "Appid|Pid|outTradeNo";
        private const char splitChar = '|';
        private readonly IAlipayClient _client;
        private readonly AlipayOptions _options;
        private string _businessContent;
        public AlipayPayQueryHandler(ILogger<AlipayPayQueryHandler> log, IAlipayClient client, IOptionsSnapshot<AlipayOptions> options)
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
                var Appid = infos[i++];
                var Pid = infos[i++];
                var outTradeNo = infos[i++];

                var queryModel = new AlipayTradeQueryModel
                {
                    OutTradeNo = outTradeNo                    
                };
                var request = new AlipayTradeQueryRequest();
                request.SetBizModel(queryModel);

                _options.AppId = Appid;
                _options.PId = Pid;
                var response = await _client.ExecuteAsync(request,_options);
                var result = response.FailResult();

                if (response.IsSuccessCode())
                {
                    var tradeStatus = response.TradeStatus;
                    if (tradeStatus == "TRADE_SUCCESS" || tradeStatus == "TRADE_FINISHED")
                    {
                        var tradeNo = response.TradeNo;
                        var paidTime = response.SendPayDate;
                        var paidAmount = response.ReceiptAmount;
                        var resultStrByQuery = string.Format("{0}|{1}|{2}", tradeNo, paidTime, paidAmount);
                        return HandleResult.Success(resultStrByQuery);
                    } else if (tradeStatus == "TRADE_CLOSED")
                    {
                        result = HandleResult.Fail("未付款交易超时关闭，或支付完成后全额退款");
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
