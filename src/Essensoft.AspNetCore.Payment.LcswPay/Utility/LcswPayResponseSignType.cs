namespace Essensoft.AspNetCore.Payment.LcswPay.Utility
{
    /// <summary>
    /// 利楚商务扫呗返回结果签名计算方式
    /// </summary>
    public enum LcswPayResponseSignType
    {
        /// <summary>
        /// 在返回值是01时，计算文档中描述中指定的参与签名计算的参数
        /// </summary>
        CalcSpecialParasWhenReturnCodeIs01,
        /// <summary>
        /// 所有非空参数，按字典序进行计算
        /// </summary>
        AllNotNullParas
    }
}
