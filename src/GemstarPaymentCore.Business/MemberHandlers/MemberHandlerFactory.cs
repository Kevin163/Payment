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
        /// <returns>对应的会员处理实例</returns>
        public IMemberHandler GetMemberHandler(string memberType, string memberUrl)
        {
            if (memberType.Equals("gsclub", StringComparison.InvariantCultureIgnoreCase) || memberType.Equals("webgscrm", StringComparison.InvariantCultureIgnoreCase))
            {
                return new WebGSCrm.WebGSCrmMemberHandler(_httpClientFactory, memberUrl);
            }
            throw new ArgumentOutOfRangeException(nameof(memberType), $"不支持指定的会员类型{memberType}");
        }
    }
}
