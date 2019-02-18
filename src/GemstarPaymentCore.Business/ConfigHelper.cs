using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
