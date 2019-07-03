namespace Essensoft.AspNetCore.Payment.LcswPay.Utility
{
    /// <summary>
    /// 利楚商务扫呗支付签名方式
    /// </summary>
    public enum LcswPaySignType
    {
        /// <summary>
        /// 计算所有必传参数
        /// </summary>
        AllRequiredPara,
        /// <summary>
        /// 计算所有必传参数和令牌
        /// </summary>
        AllRequiredParaAndToken,
        /// <summary>
        /// 按字典序计算所有非null参数和令牌
        /// </summary>
        AllNotNullParaAndToken,
    }
}
