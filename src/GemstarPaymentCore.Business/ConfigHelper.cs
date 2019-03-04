namespace GemstarPaymentCore.Business
{
    /// <summary>
    /// 配置的辅助类
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// 微信支付需要证书时的证书名称，用于在配置和实例化时取相同的名称
        /// </summary>
        public static string WxPayCertificateName = "WxPayCertificateName";
        /// <summary>
        /// 微信服务商AppId
        /// </summary>
        public static string WxProviderAppId = "wx4cea1ae3f21c72e8";
        /// <summary>
        /// 微信服务商商户号
        /// </summary>
        public static string WxProviderMchId = "1345752201";
        /// <summary>
        /// 微信服务商key
        /// </summary>
        public static string WxProviderKey = "ShenZhenJieXinDa4007755123364567";
    }
}
