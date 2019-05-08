namespace GemstarPaymentCore.Business.MemberHandlers
{
    /// <summary>
    /// 会员处理类工厂
    /// </summary>
    public interface IMemberHandlerFactory
    {
        /// <summary>
        /// 获取指定会员类型对应的会员处理实例
        /// </summary>
        /// <param name="memberType">会员类型</param>
        /// <param name="memberUrl">会员接口地址</param>
        /// <returns>对应的会员处理实例</returns>
        IMemberHandler GetMemberHandler(string memberType,string memberUrl);
    }
}
