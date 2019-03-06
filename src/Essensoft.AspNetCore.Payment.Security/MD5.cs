using System;
using System.Text;

namespace Essensoft.AspNetCore.Payment.Security
{
    public class MD5
    {
        public static string Compute(string data)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hsah = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hsah).Replace("-", "");
            }
        }
    }

    /// <summary>
    /// md5加密运算辅助类
    /// </summary>
    public static class MD5Helper
    {
        /// <summary>
        /// md5运算结果长度
        /// </summary>
        public enum MD5Length : int
        {
            Length16 = 16,
            Length32 = 32,
            LengthAll = 0
        }
        /// <summary>
        /// MD5加密,返回指定长度的小写字符串
        /// </summary>
        /// <param name="Text">待加密字串</param>
        /// <param name="encoding">字符集</param>
        /// <param name="length">16或32值之一,其它则采用.net默认MD5加密算法</param>
        /// <returns>加密后的字串</returns>
        public static string MD5Encrypt(string Text, Encoding encoding, MD5Length length = MD5Length.Length32)
        {
            try
            {
                byte[] bytes = encoding.GetBytes(Text);
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    var hashValue = md5.ComputeHash(bytes);
                    StringBuilder sb = new StringBuilder();
                    switch (length)
                    {
                        case MD5Length.Length16:
                            for (int i = 4; i < 12; i++)
                                sb.Append(hashValue[i].ToString("x2"));
                            break;
                        case MD5Length.Length32:
                            for (int i = 0; i < 16; i++)
                            {
                                sb.Append(hashValue[i].ToString("x2"));
                            }
                            break;
                        default:
                            for (int i = 0; i < hashValue.Length; i++)
                            {
                                sb.Append(hashValue[i].ToString("x2"));
                            }
                            break;
                    }
                    return sb.ToString();
                }

            } catch { return Text; }
        }
    }
}


