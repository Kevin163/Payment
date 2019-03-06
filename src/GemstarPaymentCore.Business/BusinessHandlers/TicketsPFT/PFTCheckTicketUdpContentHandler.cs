using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.TicketsPFT
{
    /// <summary>
    /// 票付通核销验票接口
    /// </summary>
    public class PFTCheckTicketUdpContentHandler : PFTBase, IBusinessHandler
    {
        private string _content;
        private ILogger _log;
        private IHttpClientFactory _clientFactory;
        private BusinessOption _businessOption;
        public PFTCheckTicketUdpContentHandler(ILogger<PFTCheckTicketUdpContentHandler> logger, IHttpClientFactory httpClientFactory, IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = logger;
            _clientFactory = httpClientFactory;
            _businessOption = businessOption.Value;
        }

        public void SetBusinessContent(string businessContent)
        {
            _content = businessContent;
        }
        public async Task<HandleResult> HandleBusinessContentAsync()
        {
            try
            {
                var switchUrl = _businessOption.PFTSwitchUrl;
                if (string.IsNullOrWhiteSpace(switchUrl))
                {
                    return HandleResult.Fail("请先在配置文件中增加PFTSwitchUrl参数设置，用于与票付通进行通信");
                }
                var contentInfo = _content.Split('|');
                var contentFormat = "pftAppId|pftSecret|pftSalerId|checkType|checkValue";
                var contentFormatLength = contentFormat.Split('|').Length;
                var length = contentInfo.Length;
                if (length < contentFormatLength)
                {
                    return HandleResult.Fail(string.Format("参数格式不正确，请按照{0}格式进行传递", contentFormat));
                }
                var appId = contentInfo[0];
                var secret = contentInfo[1];
                var salerId = contentInfo[2];
                var checkType = contentInfo[3];
                var checkValue = contentInfo[4];

                if (string.IsNullOrWhiteSpace(checkValue))
                {
                    return HandleResult.Fail("请指定验证类型对应的值，不能为空");
                }

                var paras = new StringBuilder();
                paras.Append("{\"check_type\":\"").Append(checkType).Append("\"")
                    .Append(",\"salerid\":\"").Append(salerId).Append("\"");
                if (checkType == "1")
                {
                    paras.Append(",\"code\":\"").Append(checkValue).Append("\"}");
                } else if (checkType == "2")
                {
                    paras.Append(",\"mobile\":\"").Append(checkValue).Append("\"}");
                } else if (checkType == "3")
                {
                    paras.Append(",\"idcard\":\"").Append(checkValue).Append("\"}");
                } else
                {
                    paras.Append(",\"order_batch\":\"").Append(checkValue).Append("\"}");
                }
                var method = "Check_Order";
                string data = paras.ToString();
                string timestamp = GetTimeStamp();
                string base64data = Base64Encrypt(data);
                string signature = GetSignature(method, secret, timestamp, base64data);
                string postdata = "{\"method\":\"" + method + "\",\"app_id\":\"" + appId + "\",\"timestamp\":\"" + timestamp + "\",\"signature\":\"" + signature + "\",\"params\":\"" + base64data + "\"}";
                _log.LogInformation("PFTCheckTicketUdpContentHandler", string.Format("发送的请求字符串:{0},未编码的参数：{1}", postdata, data));

                using (var client = _clientFactory.CreateClient())
                {
                    string result = await SendPost(client, switchUrl, postdata);

                    _log.LogInformation("PFTCheckTicketUdpContentHandler", string.Format("接收到的返回字符串:{0}", result));
                    var resultObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
                    if (resultObj == null)
                    {
                        return HandleResult.Fail("接收到的数据格式不能正确处理");
                    }
                    if (!(bool)resultObj["Success"])
                    {
                        return HandleResult.Fail(resultObj["Data"].ToString());
                    }
                    resultObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultObj["Data"].ToString());
                    if (resultObj["code"] == null || resultObj["code"].ToString() != "200")
                    {
                        return HandleResult.Fail(resultObj["msg"].ToString());
                    }
                    var resultdata = resultObj["data"];
                    var resultDataStr = JsonConvert.SerializeObject(resultdata);
                    return HandleResult.Success(resultDataStr);
                }

            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }

    }
}
