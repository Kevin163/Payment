﻿using Essensoft.AspNetCore.Payment.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GemstarPaymentCore.Business.Utility;
using System.Xml;
using System.Linq;
using System.Data.SqlClient;

namespace GemstarPaymentCore.Business.BusinessHandlers.TicketsGRGBooking
{
    /// <summary>
    /// 广电运通订单查询接口
    /// </summary>
    public class GRGBookingOrderQueryUdpContentHandler : IBusinessHandler
    {
        private string _content;
        private ILogger _log;
        private IHttpClientFactory _clientFactory;
        private BusinessOption _businessOption;
        public GRGBookingOrderQueryUdpContentHandler(ILogger<GRGBookingOrderQueryUdpContentHandler> logger, IHttpClientFactory httpClientFactory, IOptionsSnapshot<BusinessOption> businessOption)
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
                var switchUrl = _businessOption.GRGBookingAPIUrl;
                if (string.IsNullOrWhiteSpace(switchUrl))
                {
                    return HandleResult.Fail("请先在配置文件中增加GRGBookingAPIUrl参数设置，用于与广电运通进行通信");
                }
                switchUrl = System.IO.Path.Combine(switchUrl, "queryOrder.do");

                var contentInfo = _content.Split('|');
                var contentFormat = "GRGCorpCode|GRGCorpKey|GRGUsername|orderNo|systemName";
                var contentFormatLength = contentFormat.Split('|').Length;
                var length = contentInfo.Length;
                if (length < contentFormatLength)
                {
                    return HandleResult.Fail(string.Format("参数格式不正确，请按照{0}格式进行传递", contentFormat));
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
                _log.LogInformation("GRGBookingOrderQueryUdpContentHandler", string.Format("发送给广电运通的订单查询字符串：\r\n" + sendStr));
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
                _log.LogInformation("GRGBookingOrderQueryUdpContentHandler", string.Format("接收到广电运通的订单查询结果：\r\n" + result));
                var resultDoc = new XmlDocument();
                resultDoc.LoadXml(result);
                var codeElement = resultDoc.GetElementsByTagName("code");
                var codeValue = codeElement[0].InnerText;
                if (codeValue == "0")
                {
                    try
                    {
                        var businessSystemInfo = _businessOption.Systems.FirstOrDefault(w=>w.Name.Equals(systemName,StringComparison.OrdinalIgnoreCase));
                        if (businessSystemInfo == null || businessSystemInfo.HavePay != 1 || string.IsNullOrWhiteSpace(businessSystemInfo.ConnStr))
                        {
                            return HandleResult.Fail($"业务系统{systemName}未启用支付或者未设置对应的数据库连接信息");
                        }
                        //由于订单的内容很多，需要考虑一下看如何才能有效的返回
                        using (var conn = new SqlConnection(businessSystemInfo.ConnStr))
                        {
                            var cmd = conn.CreateCommand();
                            cmd.CommandText = "up_itf_GRGOrder_xmlHandle";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            var pXml = cmd.CreateParameter();
                            pXml.ParameterName = "@orderXml";
                            pXml.SqlDbType = System.Data.SqlDbType.Text;
                            pXml.SqlValue = result;
                            cmd.Parameters.Add(pXml);

                            conn.Open();
                            var businessId = cmd.ExecuteScalar().ToString();
                            conn.Close();
                            return HandleResult.Success(businessId);
                        }
                    }
                    catch(Exception ex)
                    {
                        return HandleResult.Fail("执行存储过程up_itf_GRGOrder_xmlHandle时遇到错误，"+ex.Message);
                    }
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