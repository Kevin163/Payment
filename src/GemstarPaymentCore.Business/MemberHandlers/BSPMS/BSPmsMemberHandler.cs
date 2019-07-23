using Essensoft.AspNetCore.Payment.Security;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GemstarPaymentCore.Business.MemberHandlers.BSPMS
{
    public class BSPmsMemberHandler: IMemberHandler
    {
        private IHttpClientFactory _httpClientFactory;
        private string _memberUrl;
        private BSPmsMemberPara _para;
        public BSPmsMemberHandler(IHttpClientFactory httpClientFactory,string memberUrl,BSPmsMemberPara para)
        {
            _httpClientFactory = httpClientFactory;
            _memberUrl = memberUrl;
            _para = para;
        }
        public async Task<List<MemberInfo>> QueryMember(string openId)
        {
            var xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"gbk\"?>")
                .Append("<RealOperate>")
                .Append("<XType>JxdBSPms</XType>")
                .Append("<OpType>会员查询</OpType>")
                .Append("<MbrQuery>")
                .Append("<OtherKeyWord>").Append(openId).Append("</OtherKeyWord>")
                .Append("<OtherName>微信</OtherName>")
                .Append("</MbrQuery>")
                .Append("</RealOperate>");

            var httpClient = _httpClientFactory.CreateClient();
            var url = AddQueryParameters(xml.ToString());

            using (var requestContent = new StringContent(xml.ToString(), Encoding.UTF8, "text/xml"))
            using (var response = await httpClient.PostAsync(url, requestContent))
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
                .Append("<XType>JxdBSPms</XType>")
                .Append("<OpType>会员交易</OpType>")
                .Append("<ProfileCa>")
                .Append("<BalanceType>0102</BalanceType>")
                .Append("<Amount>").Append(para.Amount > 0 ? -para.Amount : para.Amount).Append("</Amount>")
                .Append("<Creator>jxdUnionPay</Creator>")
                .Append("<RefNo>").Append(para.RefNo).Append("</RefNo>")
                .Append("<ProfileId>").Append(para.Id).Append("</ProfileId>")
                .Append("<CPW>").Append(CalcMemberPwd(para.Id,para.Password)).Append("</CPW>")
                .Append("<Remark>").Append(para.Remark).Append("</Remark>")
                .Append("<PaymentDesc>02</PaymentDesc>")
                .Append("</ProfileCa>")
                .Append("</RealOperate>");

            var httpClient = _httpClientFactory.CreateClient();
            var url = AddQueryParameters(xml.ToString());

            using (var requestContent = new StringContent(xml.ToString(), Encoding.UTF8, "text/xml"))
            using (var response = await httpClient.PostAsync(url, requestContent))
            using (var responseContent = response.Content)
            {
                var resultStr = await responseContent.ReadAsStringAsync();
                return ParsePaymentResult(resultStr);
            }
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

        private static List<MemberInfo> ParseQueryResult(string resultStr)
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
                    }
                }
                result.Add(member);
            }
            return result;
        }
        /// <summary>
        /// 在链接地址中加入调用bs捷云会员需要的参数，并且计算签名
        /// </summary>
        /// <param name="xmlStr">要传递的xml字符串</param>
        /// <returns>将通用参数和签名值附加到地址中</returns>
        private string AddQueryParameters(string xmlStr)
        {
            var url = new StringBuilder(_memberUrl);
            var signStr = $"{_para.ChannelKey}{_para.GrpId}{_para.ChannelCode}{xmlStr}";
            var signValue = MD5Helper.MD5Encrypt(signStr, Encoding.UTF8, MD5Helper.MD5Length.Length32);

            var split = _memberUrl.Contains("?") ? "&" : "?";
            url.Append(split).Append("grpid=").Append(_para.GrpId)
                .Append("&channel=").Append(_para.ChannelCode)
                .Append("&sign=").Append(signValue);

            return url.ToString();
        }
        /// <summary>
        /// 计算会员的密码，以便和会员数据库中的加密密码进行匹配
        /// 此处要求加密密码计算方式需要与捷云会员中的加密方式保持一致
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        private string CalcMemberPwd(string profileId,string pwd)
        {
            if (string.IsNullOrEmpty(pwd))
            {
                return string.Empty;
            }
            var signStr = $"{profileId.ToLower()}{pwd}";
            return MD5Helper.MD5Encrypt(signStr, Encoding.UTF8, MD5Helper.MD5Length.Length32);
        }
    }
}
