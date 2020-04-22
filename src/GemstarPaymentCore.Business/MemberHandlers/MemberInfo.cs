namespace GemstarPaymentCore.Business.MemberHandlers
{
    /// <summary>
    /// 会员信息
    /// </summary>
    public class MemberInfo
    {
        /// <summary>
        /// 会员主键
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 会员卡类型代码
        /// </summary>
        public string CardTypeNo { get; set; }
        /// <summary>
        /// 会员卡类型名称
        /// </summary>
        public string CardTypeName { get; set; }
        /// <summary>
        /// 会员卡号
        /// </summary>
        public string CardNo { get; set; }
        /// <summary>
        /// 中文姓名
        /// </summary>
        public string CName { get; set; }
        /// <summary>
        /// 英文名称
        /// </summary>
        public string EName { get; set; }
        /// <summary>
        /// 会员手机号
        /// </summary>
        public string MobileNo { get; set; }
        /// <summary>
        /// 会员可用储值余额
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 会员可用积分
        /// </summary>
        public decimal Score { get; set; }
        /// <summary>
        /// 是否拥有密码
        /// </summary>
        public bool HavePassword { get; set; }
    }
}
