namespace GemstarPaymentCore.Business.MemberHandlers.BSPMS
{
    /// <summary>
    /// 捷云会员相关参数
    /// </summary>
    public class BSPmsMemberPara
    {
        /// <summary>
        /// 集团id
        /// </summary>
        public string GrpId { get; set; }
        /// <summary>
        /// 渠道代码
        /// </summary>
        public string ChannelCode { get; set; }
        /// <summary>
        /// 渠道密钥
        /// </summary>
        public string ChannelKey { get; set; }
    }
}
