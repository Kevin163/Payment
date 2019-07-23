using GemstarPaymentCore.Business.MemberHandlers.BSPMS;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace GemstarPaymentCore.Business.MemberHandlers
{
    public class MemberHandlerFactory : IMemberHandlerFactory
    {
        private IHttpClientFactory _httpClientFactory;
        public MemberHandlerFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 获取指定会员类型对应的会员处理实例
        /// </summary>
        /// <param name="memberType">会员类型</param>
        /// <param name="memberUrl">会员接口地址</param>
        /// <param name="memberPara">会员参数</param>
        /// <returns>对应的会员处理实例</returns>
        public IMemberHandler GetMemberHandler(string memberType, string memberUrl, string memberPara)
        {
            if (IsMemberWebGsCrm(memberType))
            {
                return new WebGSCrm.WebGSCrmMemberHandler(_httpClientFactory, memberUrl);
            }
            else if (IsMemberBspms(memberType))
            {
                if (string.IsNullOrEmpty(memberPara))
                {
                    throw new ArgumentNullException(nameof(memberPara), "捷云会员必须指定集团id，渠道代码和渠道密钥");
                }
                var para = JsonConvert.DeserializeObject<BSPmsMemberPara>(memberPara);
                if (para == null || string.IsNullOrEmpty(para.GrpId) || string.IsNullOrEmpty(para.ChannelCode) || string.IsNullOrEmpty(para.ChannelKey))
                {
                    throw new ArgumentOutOfRangeException(nameof(memberPara), "捷云会员必须指定有效的集团id，渠道代码和渠道密钥");
                }
                return new BSPmsMemberHandler(_httpClientFactory, memberUrl, para);
            }
            throw new ArgumentOutOfRangeException(nameof(memberType), $"不支持指定的会员类型{memberType}");
        }
        /// <summary>
        /// 是否是捷云会员
        /// </summary>
        /// <param name="memberType">会员类型</param>
        /// <returns>true:是捷云会员,false:不是</returns>
        public static bool IsMemberBspms(string memberType)
        {
            return memberType.Equals("bspms", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// 是否是老版本的cs或bs会员
        /// </summary>
        /// <param name="memberType">会员类型</param>
        /// <returns>true:是老版本的cs或bs会员，false:不是</returns>
        public static bool IsMemberWebGsCrm(string memberType)
        {
            return memberType.Equals("gsclub", StringComparison.InvariantCultureIgnoreCase) || memberType.Equals("webgscrm", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
