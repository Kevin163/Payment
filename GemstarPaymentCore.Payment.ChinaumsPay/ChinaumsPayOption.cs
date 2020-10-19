namespace GemstarPaymentCore.Payment.ChinaumsPay
{
    /// <summary>
    /// 银商支付配置信息
    /// </summary>
    public class ChinaumsPayOption
    {
        /// <summary>
        /// 测试环境下的api地址
        /// </summary>
        private const string ApiUrlForEnvironmentTest = "https://qr-test2.chinaums.com/netpay-route-server/api/";
        /// <summary>
        /// 生产环境的api地址
        /// </summary>
        private const string ApiUrlForEnvironmentProduction = "https://qr.chinaums.com/netpay-route-server/api/";
        /// <summary>
        /// 商户号
        /// </summary>
        public string MId { get; set; }
        /// <summary>
        /// 终端号
        /// </summary>
        public string TId { get; set; }
        /// <summary>
        /// 签名密钥
        /// </summary>
        public string SignKey { get; set; }
        /// <summary>
        /// 是否测试环境，1：测试环境，0：正式环境
        /// </summary>
        public int IsEnvTest { get; set; }
        /// <summary>
        /// 获取当前环境的接口地址
        /// </summary>
        public string ApiUrl => IsEnvTest == 1 ? ApiUrlForEnvironmentTest : ApiUrlForEnvironmentProduction;
    }
}
