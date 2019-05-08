namespace GemstarPaymentCore.Models
{
    public class JxdUnionPayViewModel
    {
        /// <summary>
        /// 是否参数正常
        /// </summary>
        public bool IsParaOK { get; set; }
        /// <summary>
        /// 不正常的提示信息
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 扫呗支付聚合二维码支付地址
        /// </summary>
        public string LcswPayQrcodeUrl { get; set; }
        /// <summary>
        /// 微信公众号appid
        /// </summary>
        public string AppId { get; set; }
    }
}
