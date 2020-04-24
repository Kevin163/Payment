﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GemstarPaymentCore.Business.MemberHandlers.WebGSCrm
{
    public class WebGSCrmMemberHandler : IMemberHandler
    {
        private IHttpClientFactory _httpClientFactory;
        private string _memberUrl;
        public WebGSCrmMemberHandler(IHttpClientFactory httpClientFactory,string memberUrl)
        {
            _httpClientFactory = httpClientFactory;
            _memberUrl = memberUrl;
        }
        public async Task<List<MemberInfo>> QueryMember(string openId)
        {
            var xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"gbk\"?>")
                .Append("<RealOperate>")
                .Append("<XType>GemStar</XType>")
                .Append("<OpType>会员查询</OpType>")
                .Append("<MbrQuery>")
                .Append("<OtherKeyWord>").Append(openId).Append("</OtherKeyWord>")
                .Append("<OtherName>微信</OtherName>")
                .Append("</MbrQuery>")
                .Append("</RealOperate>");

            var httpClient = _httpClientFactory.CreateClient();

            using (var requestContent = new StringContent(xml.ToString(), Encoding.UTF8, "text/xml"))
            using (var response = await httpClient.PostAsync(_memberUrl, requestContent))
            using (var responseContent = response.Content)
            {
                var resultStr = await responseContent.ReadAsStringAsync();
                return ParseQueryResult(resultStr);
            }
        }
        /// <summary>
        /// 会员交易，扣减会员储值余额
        /// </summary>
        /// <param name="para">交易参数实例</param>
        /// <param name="errorMessage">失败时的提示信息</param>
        /// <returns>true:扣款成功，false:扣款失败</returns>
        public async Task<MemberPaymentResult> MemberPayment(MemberPaymentParameter para)
        {
            var xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"gbk\"?>")
                .Append("<RealOperate>")
                .Append("<XType>GemStar</XType>")
                .Append("<OpType>会员交易</OpType>")
                .Append("<ProfileCa>")
                .Append("<Amount>").Append(para.Amount > 0 ? -para.Amount : para.Amount).Append("</Amount>")
                .Append("<OrigBillNo>").Append(para.OrigBillNo).Append("</OrigBillNo>")
                .Append("<ResortId>0</ResortId>")
                .Append("<Creator>payment</Creator>")
                .Append("<RefNo>").Append(para.RefNo).Append("</RefNo>")
                .Append("<ProfileId>").Append(para.Id).Append("</ProfileId>")
                .Append("<Remark>").Append(para.Remark).Append("</Remark>")
                .Append("<PaymentDesc>12</PaymentDesc>")
                .Append("<CardPassWord>").Append(para.Password).Append("</CardPassWord>")
                .Append("</ProfileCa>")
                .Append("</RealOperate>");

            var httpClient = _httpClientFactory.CreateClient();

            using (var requestContent = new StringContent(xml.ToString(), Encoding.UTF8, "text/xml"))
            using (var response = await httpClient.PostAsync(_memberUrl, requestContent))
            using (var responseContent = response.Content)
            {
                var resultStr = await responseContent.ReadAsStringAsync();
                return ParsePaymentResult(resultStr);
            }
        }

        public async Task<List<MemberTicketInfo>> QueryMemberTickets(MemberTicketQueryParameter para)
        {
            if (string.IsNullOrEmpty(para.Id))
            {
                return new List<MemberTicketInfo>();
            }
            var xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"gbk\"?>")
                .Append("<RealOperate>")
                .Append("<XType>GemStar</XType>")
                .Append("<OpType>会员券明细查询</OpType>")
                .Append("<ProfileId>").Append(para.Id).Append("</ProfileId>")
                .Append("<outletcode>").Append(para.OutletCode).Append("</outletcode>")
                .Append("<OnlyChargeTicket>1</OnlyChargeTicket>")
                .Append("<isValid>1</isValid>")
                .Append("</RealOperate>");

            var httpClient = _httpClientFactory.CreateClient();

            using (var requestContent = new StringContent(xml.ToString(), Encoding.UTF8, "text/xml"))
            using (var response = await httpClient.PostAsync(_memberUrl, requestContent))
            using (var responseContent = response.Content)
            {
                var resultStr = await responseContent.ReadAsStringAsync();
                return ParseQueryTicketResult(resultStr);
            }
        }

        public async Task<MemberPaymentResult> MemberUseTicket(MemberTicketUseParameter para)
        {
            var xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"gbk\"?>")
                .Append("<RealOperate>")
                .Append("<XType>GemStar</XType>")
                .Append("<OpType>会员券核销</OpType>")
                .Append("<ProfileId>").Append(para.Id).Append("</ProfileId>")
                .Append("<Ticketcode>").Append(para.TicketCode).Append("</Ticketcode>")
                .Append("<operator>").Append(para.Operator).Append("</operator>")
                .Append("<useoutletcode>").Append(para.OutletCode).Append("</useoutletcode>")
                .Append("<RefNo>").Append(para.RefNo).Append("</RefNo>")
                .Append("</RealOperate>");

            var httpClient = _httpClientFactory.CreateClient();

            using (var requestContent = new StringContent(xml.ToString(), Encoding.UTF8, "text/xml"))
            using (var response = await httpClient.PostAsync(_memberUrl, requestContent))
            using (var responseContent = response.Content)
            {
                var resultStr = await responseContent.ReadAsStringAsync();
                return ParseTicketUseResult(resultStr);
            }
        }

        public async Task<MemberPaymentResult> MemberUnusedTicket(MemberTicketUnusedParameter para)
        {
            var xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"gbk\"?>")
                .Append("<RealOperate>")
                .Append("<XType>GemStar</XType>")
                .Append("<OpType>会员券反核销</OpType>")
                .Append("<ProfileId>").Append(para.Id).Append("</ProfileId>")
                .Append("<Ticketcode>").Append(para.TicketCode).Append("</Ticketcode>")
                .Append("<remark>").Append(para.Remark).Append("</remark>")
                .Append("</RealOperate>");

            var httpClient = _httpClientFactory.CreateClient();

            using (var requestContent = new StringContent(xml.ToString(), Encoding.UTF8, "text/xml"))
            using (var response = await httpClient.PostAsync(_memberUrl, requestContent))
            using (var responseContent = response.Content)
            {
                var resultStr = await responseContent.ReadAsStringAsync();
                return ParseTicketUseResult(resultStr);
            }
        }
        private List<MemberTicketInfo> ParseQueryTicketResult(string resultStr)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resultStr);
            var rows = xmlDocument.GetElementsByTagName("Row");
            var result = new List<MemberTicketInfo>();
            foreach (XmlNode node in rows)
            {
                var member = new MemberTicketInfo();
                foreach (XmlNode child in node.ChildNodes)
                {
                    string name = child.Name;
                    string value = child.InnerText;
                    switch (name)
                    {
                        case "ProfileId":
                            member.Id = value;
                            break;
                        case "seqid":
                            member.TicketCode = value;
                            break;
                        case "TicketType":
                            member.TicketTypeCode = value;
                            break;
                        case "TicketTypeCname":
                            member.TicketTypeCname = value;
                            break;
                        case "useflag":
                            member.IsUsed = Convert.ToInt32(value) == 1;
                            break;
                        case "Chargeamt":
                            member.Amount = Convert.ToDecimal(value);
                            break;
                        case "limitdate":
                            member.ExpiredDate = Convert.ToDateTime(value);
                            break;
                    }
                }
                result.Add(member);
            }
            return result;
        }

        private MemberPaymentResult ParsePaymentResult(string resultStr)
        {
            var result = new MemberPaymentResult
            {
                PaySuccess = false,
                Message = ""
            };
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resultStr);
            var messageNode = xmlDocument.GetElementsByTagName("Message");
            if(messageNode.Count > 0)
            {
                result.Message = messageNode[0].InnerText;
            }
            var messageNoNode = xmlDocument.GetElementsByTagName("MessageNo");
            if(messageNoNode.Count > 0)
            {
                var messageNoValue = messageNoNode[0].InnerText;
                result.PaySuccess =  messageNoValue == "1";
            }
            return result;
        }

        private List<MemberInfo> ParseQueryResult(string resultStr)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resultStr);
            var rows = xmlDocument.GetElementsByTagName("Row");
            var result = new List<MemberInfo>();
            foreach(XmlNode node in rows)
            {
                var member = new MemberInfo();
                foreach(XmlNode child in node.ChildNodes)
                {
                    string name = child.Name;
                    string value = child.InnerText;
                    switch (name)
                    {
                        case "ProfileId":
                            member.Id = value;
                            break;
                        case "MbrCardType":
                            member.CardTypeNo = value;
                            break;
                        case "MbrCardTypeName":
                            member.CardTypeName = value;
                            break;
                        case "MbrCardNo":
                            member.CardNo = value;
                            break;
                        case "GuestCName":
                            member.CName = value;
                            break;
                        case "GuestEName":
                            member.EName = value;
                            break;
                        case "Mobile":
                            member.MobileNo = value;
                            break;
                        case "Balance":
                            member.Balance = Convert.ToDecimal(value);
                            break;
                        case "ValidScore":
                            member.Score = Convert.ToDecimal(value);
                            break;
                        case "havePassword":
                            member.HavePassword = Convert.ToInt32(value) == 1;
                            break;
                    }
                }
                result.Add(member);
            }
            return result;
        }

        private MemberPaymentResult ParseTicketUseResult(string resultStr)
        {
            var result = new MemberPaymentResult
            {
                PaySuccess = false,
                Message = ""
            };
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resultStr);
            var messageNode = xmlDocument.GetElementsByTagName("rtninfo");
            if (messageNode.Count > 0)
            {
                result.Message = messageNode[0].InnerText;
            }
            var messageNoNode = xmlDocument.GetElementsByTagName("rtncode");
            if (messageNoNode.Count > 0)
            {
                var messageNoValue = messageNoNode[0].InnerText;
                result.PaySuccess = messageNoValue == "0";
            }
            return result;
        }

    }
}
