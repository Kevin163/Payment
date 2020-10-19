using System;
using System.Collections.Generic;
using System.Text;

namespace GemstarPaymentCore.Payment.ChinaumsPay
{
    /// <summary>
    /// 银商支付请求接口
    /// </summary>
    public interface IChinaumsPayRequest<T> where T:ChinaumsPayResponse
    {
        /// <summary>
        /// 获取请求的所有参数
        /// </summary>
        /// <returns>请求参数数组</returns>
        ChinaumsPayDictionary GetParameters();
    }
}
