using Essensoft.AspNetCore.Payment.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GemstarPaymentCore.Business.Utility;
using System.Xml;

namespace GemstarPaymentCore.Business.BusinessHandlers.TicketsGRGBooking
{
    /// <summary>
    /// 广电运通订单明细核销
    /// </summary>
    public class GRGBookingOrderCheckUdpContentHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private IHttpClientFactory _clientFactory;
        private BusinessOption _businessOption;
        public GRGBookingOrderCheckUdpContentHandler(ILogger<GRGBookingOrderCheckUdpContentHandler> logger, IHttpClientFactory httpClientFactory, IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = logger;
            _clientFactory = httpClientFactory;
            _businessOption = businessOption.Value;
        }
        protected override string contentFormat => "GRGCorpCode|GRGCorpKey|GRGUsername|orderDetailId|checkNum";
        protected override int[] contentEncryptedIndexs => new int[] { };
        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] contentInfo)
        {
            try
            {
                var switchUrl = _businessOption.GRGBookingAPIUrl;
                if (string.IsNullOrWhiteSpace(switchUrl))
                {
                    return HandleResult.Fail("请先在配置文件中增加GRGBookingAPIUrl参数设置，用于与广电运通进行通信");
                }
                switchUrl = System.IO.Path.Combine(switchUrl, "checkTicket.do");

                var corpCode = contentInfo[0];
                var corpKey = contentInfo[1];
                var username = contentInfo[2];
                var orderDetailId = contentInfo[3];
                var checknum = contentInfo[4];

                var xmlContent = new StringBuilder();
                xmlContent.Append("xmlMsg=<PWBRequest>")
                    .Append("<transactionName>THIRD_CHECK_TICKET_REQ</transactionName>")
                    .Append("<header>")
                    .Append("<application>SendCode</application>")
                    .Append("<requestTime>").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("</requestTime>")
                    .Append("</header>")
                    .Append("<identityInfo>")
                    .Append("<corpCode>").Append(corpCode).Append("</corpCode>")
                    .Append("<userName>").Append(username).Append("</userName>")
                    .Append("</identityInfo>")
                    .Append("<checkTicket>")
                    .Append("<orderDetailId>").Append(orderDetailId).Append("</orderDetailId>")
                    .Append("<checkNum>").Append(checknum).Append("</checkNum>")
                    .Append("</checkTicket>")
                    .Append("</PWBRequest>");

                var sendStr = xmlContent.ToString();
                var signStr = string.Format("{0}{1}", sendStr, corpKey);
                var sign = MD5Helper.MD5Encrypt(signStr, Encoding.UTF8, MD5Helper.MD5Length.Length32);
                xmlContent.Append("&sign=").Append(sign);
                sendStr = xmlContent.ToString();
                _log.LogInformation("GRGBookingOrderCheckUdpContentHandler", string.Format("发送给广电运通的订单明细检票字符串：\r\n" + sendStr));
                string result;
#if MOCK
                if (orderDetailId.Equals("mockDetailId"))
                {
                    result = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><PWBResponse>  <transactionName>QUERY_SCENIC_ORDERS_RES</transactionName>  <code>0</code>  <description>成功</description></PWBResponse>";
                }
                else
                {
                    result = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><PWBResponse>  <transactionName>QUERY_SCENIC_ORDERS_RES</transactionName>  <code>1</code>  <description>mock时只允许针对明细id为mockDetailId的才能检票成功</description></PWBResponse>";
                }
#else
                using (var client = _clientFactory.CreateClient())
                {
                    result = await client.DoPostAsync(switchUrl, sendStr);
                }
#endif
                _log.LogInformation("GRGBookingOrderCheckUdpContentHandler", string.Format("接收到广电运通的订单明细检票结果：\r\n" + result));
                var resultDoc = new XmlDocument();
                resultDoc.LoadXml(result);
                var codeElement = resultDoc.GetElementsByTagName("code");
                var codeValue = codeElement[0].InnerText;
                if (codeValue == "0")
                {
                    return HandleResult.Success("");
                } else
                {
                    var descElement = resultDoc.GetElementsByTagName("description");
                    return HandleResult.Fail(descElement[0].InnerText);
                }
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
