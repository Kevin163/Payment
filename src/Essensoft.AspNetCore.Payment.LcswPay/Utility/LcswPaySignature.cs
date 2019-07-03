using Essensoft.AspNetCore.Payment.Security;
using System.Collections.Generic;
using System.Text;

namespace Essensoft.AspNetCore.Payment.LcswPay.Utility
{
    /// <summary>
    /// 利楚商务签名计算
    /// </summary>
    public static class LcswPaySignature
    {
        /// <summary>
        /// 计算签名
        /// 计算时需要在参数中加上令牌后再进行计算
        /// </summary>
        /// <param name="paraInfos">要计算的参数信息</param>
        /// <param name="token">令牌</param>
        /// <returns>签名值</returns>
        public static string CalcSignWithAllRequiredParaAndToken(List<LcswPayParaInfo> paraInfos,string token)
        {
            var signContent = GetSignContentWithRequiredParas(paraInfos);
            signContent.Append($"&access_token={token}");
            return MD5.Compute(signContent.ToString());
        }
        /// <summary>
        /// 计算签名
        /// </summary>
        /// <param name="paraInfos">要计算的参数信息</param>
        /// <returns>签名值</returns>
        public static string CalcSignWithAllRequiredPara(List<LcswPayParaInfo> paraInfos)
        {
            var signContent = GetSignContentWithRequiredParas(paraInfos);
            return MD5.Compute(signContent.ToString());
        }
        /// <summary>
        /// 按字典序计算所有非空参数和令牌的签名值
        /// </summary>
        /// <param name="paraInfos">参数列表</param>
        /// <returns>计算出来的签名值</returns>
        public static string CalcSignWithAllNotNullPara(List<LcswPayParaInfo> paraInfos,bool lower = true)
        {
            var builder = GetSignContentWithNotNullParas(paraInfos);
            var sign = MD5.Compute(builder.ToString());
            if (lower)
            {
                sign = sign.ToLower();
            }
            return sign;
        }
        /// <summary>
        /// 按字典序计算所有非空参数和令牌的签名值
        /// </summary>
        /// <param name="paraInfos">参数列表</param>
        /// <param name="token">令牌</param>
        /// <returns>计算出来的签名值</returns>
        public static string CalcSignWithAllNotNullParaAndToken(List<LcswPayParaInfo> paraInfos,string token,bool lower = true)
        {
            var builder = GetSignContentWithNotNullParas(paraInfos);
            builder.Append($"&access_token={token}");
            var sign = MD5.Compute(builder.ToString());
            if (lower)
            {
                sign = sign.ToLower();
            }
            return sign;
        }

        private static StringBuilder GetSignContentWithNotNullParas(List<LcswPayParaInfo> paraInfos)
        {
            var sortedList = new SortedList<string, LcswPayParaInfo>();
            foreach (var para in paraInfos)
            {
                sortedList.Add(para.ParaName, para);
            }
            var builder = new StringBuilder();
            var split = "";
            foreach (var key in sortedList.Keys)
            {
                var para = sortedList[key];
                if (para.ParaValue != null)
                {
                    builder.Append(split).Append(para.ParaName).Append("=").Append(para.ParaValue ?? "null");
                    split = "&";
                }
            }
            return builder;
        }

        /// <summary>
        /// 将需要参与计算的参数转换为计算字符串
        /// </summary>
        /// <param name="paraInfos">要计算的参数信息</param>
        /// <returns>计算字符串</returns>
        private static StringBuilder GetSignContentWithRequiredParas(List<LcswPayParaInfo> paraInfos )
        {
            var builder = new StringBuilder();
            var split = "";
            foreach (var para in paraInfos)
            {
                if (para.IsRequired)
                {
                    builder.Append(split).Append(para.ParaName).Append("=").Append(para.ParaValue ?? "null");
                    split = "&";
                }
            }
            return builder;
        }
    }
}
