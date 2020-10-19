using System.Collections.Generic;
using System.Text;
using Essensoft.AspNetCore.Payment.Security;

namespace GemstarPaymentCore.Payment.ChinaumsPay.Utility
{
    /// <summary>
    /// 银商支付的签名工具
    /// </summary>
    public class ChinaumsPaySignature
    {
        public static string SignWithKey(SortedDictionary<string, string> dictionary, string key)
        {
            var sb = new StringBuilder();
            var split = "";
            foreach (var iter in dictionary)
            {
                if (!string.IsNullOrEmpty(iter.Value) && iter.Key != "sign" && iter.Key != "signType")
                {
                    sb.Append(split).Append(iter.Key).Append('=').Append(iter.Value);
                    split = "&";
                }
            }
            var signContent = sb.Append(key).ToString();
            return SHA256.Compute(signContent);
        }
    }
}
