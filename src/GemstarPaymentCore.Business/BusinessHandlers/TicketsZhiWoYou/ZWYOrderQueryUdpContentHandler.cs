using Essensoft.AspNetCore.Payment.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GemstarPaymentCore.Business.Utility;
using System.Xml;

namespace GemstarPaymentCore.Business.BusinessHandlers.TicketsZhiWoYou
{
    /// <summary>
    /// 自我游订单查询接口
    /// </summary>
    public class ZWYOrderQueryUdpContentHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private IHttpClientFactory _clientFactory;
        private BusinessOption _businessOption;
        public ZWYOrderQueryUdpContentHandler(ILogger<ZWYOrderQueryUdpContentHandler> logger, IHttpClientFactory httpClientFactory, IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = logger;
            _clientFactory = httpClientFactory;
            _businessOption = businessOption.Value;
        }
        protected override string contentFormat => "ZWYCorpCode|ZWYCorpKey|ZWYUsername|orderNo|systemName";
        protected override int[] contentEncryptedIndexs => new int[] { };

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] contentInfo)
        {
            try
            {
                var switchUrl = _businessOption.ZWYAPIUrl;
                if (string.IsNullOrWhiteSpace(switchUrl))
                {
                    return HandleResult.Fail("请先在配置文件中增加ZWYAPIUrl参数设置，用于与自我游进行通信");
                }

                var corpCode = contentInfo[0];
                var corpKey = contentInfo[1];
                var username = contentInfo[2];
                var orderNo = contentInfo[3];
                var systemName = contentInfo[4];

                var xmlContent = new StringBuilder();
                xmlContent.Append("xmlMsg=<PWBRequest>")
                    .Append("<transactionName>QUERY_SCENIC_ORDERS_REQ</transactionName>")
                    .Append("<header>")
                    .Append("<application>SendCode</application>")
                    .Append("<requestTime>").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("</requestTime>")
                    .Append("</header>")
                    .Append("<identityInfo>")
                    .Append("<corpCode>").Append(corpCode).Append("</corpCode>")
                    .Append("<userName>").Append(username).Append("</userName>")
                    .Append("</identityInfo>")
                    .Append("<queryParam>")
                    .Append("<checkNo>").Append(orderNo).Append("</checkNo>")
                    .Append("</queryParam>")
                    .Append("</PWBRequest>");

                var sendStr = xmlContent.ToString();
                var signStr = string.Format("{0}{1}", sendStr, corpKey);
                var sign = MD5Helper.MD5Encrypt(signStr, Encoding.UTF8, MD5Helper.MD5Length.Length32);
                xmlContent.Append("&sign=").Append(sign);
                sendStr = xmlContent.ToString();
                _log.LogInformation("ZWYOrderQueryUdpContentHandler", string.Format("发送给自我游的订单查询字符串：\r\n" + sendStr));
                string result;
#if MOCK
                //如果是模拟环境，则直接返回一个固定的订单内容
                result = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><PWBResponse>  <transactionName>QUERY_SCENIC_ORDERS_RES</transactionName>  <code>0</code>  <description>成功</description><orderContents>  <orderContent>    <id>17397159</id>    <orderCode>201602240017602258</orderCode>    <linkName>联系人</linkName>    <payMethod>vm</payMethod>    <tel>联系电话</tel>    <certificateNo>身份证号</certificateNo>    <payStatus>payed</payStatus>    <createBy>创建人</createBy>    <modifyBy>修改人</modifyBy>    <modifyTime>2016-02-24 20:40:21</modifyTime>    <needCheckNum>1</needCheckNum>    <alreadyCheckNum>0</alreadyCheckNum>    <checkStatus>un_check</checkStatus>    <thirdCode>201602240017602258</thirdCode>    <subOrderContents>      <subOrderContent>         <id>mockDetailId</id>        <scenicGoodsName>拈花湾 </scenicGoodsName>        <saleCode>001</saleCode>        <saleCorpCode>供应商编码</saleCorpCode>        <corpCode>分销商</corpCode>        <orderType>scenic</orderType>        <startDate>2016-02-27 00:00:00</startDate>        <endDate>2016-02-28 00:00:00</endDate>        <goodsName>拈花湾</goodsName>        <isVm>F</isVm>        <occDate>2016-02-27 00:00:00</occDate>        <quantity>1</quantity>        <price>810</price>        <totalPrice>810</totalPrice>        <closePrice>810</closePrice>        <closeTotalPrice>810</closeTotalPrice>        <goodsCode>PST20160212033334</goodsCode>        <payMethod>vm</payMethod>        <weekDay>1,2,3,4,5,6,7</weekDay>        <createBy>创建人</createBy>        <createTime>2016-02-24 20:40:19</createTime>        <modifyBy>修改人</modifyBy>        <modifyTime>2016-02-24 20:40:21</modifyTime>        <remarkDist>备注</remarkDist>        <alreadyCheckNum>0</alreadyCheckNum>        <needCheckNum>1</needCheckNum>        <checkStatus>un_check</checkStatus>        <checkNum>0</checkNum>        <onlineCheckNum>0</onlineCheckNum>        <returnNum>0</returnNum>        <retrunFee>0</retrunFee>        <returnFeeCategory>fixed</returnFeeCategory>        <returnFeeRate>0</returnFeeRate>        <isRetreat>T</isRetreat>        <isExpiredRetreat>T</isExpiredRetreat>        <isRetreatAudit>F</isRetreatAudit>      </subOrderContent>    </subOrderContents>  </orderContent></orderContents></PWBResponse>";
#else
                using (var client = _clientFactory.CreateClient())
                {
                    result = await client.DoPostAsync(switchUrl, sendStr);
                }
#endif
                _log.LogInformation("ZWYOrderQueryUdpContentHandler", string.Format("接收到自我游的订单查询结果：\r\n" + result));
                var resultDoc = new XmlDocument();
                resultDoc.LoadXml(result);
                var codeElement = resultDoc.GetElementsByTagName("code");
                var codeValue = codeElement[0].InnerText;
                if (codeValue == "0")
                {
                    //慢慢会过渡到使用http协议，所以此处不用担心数据太多导致无法正常接收，直接返回
                    return HandleResult.Success(result);
                }
                else
                {
                    var descElement = resultDoc.GetElementsByTagName("description");
                    return HandleResult.Fail(descElement[0].InnerText);
                }
            }
            catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }

    }
}
