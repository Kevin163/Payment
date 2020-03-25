namespace GemstarPaymentCore.Business.Utility
{
    /// <summary>
    /// 安全接口，用于加密解密字符串
    /// </summary>
    public interface ISecurity
    {
        /// <summary>
        /// 对指定的明文字符串使用指定密钥进行加密
        /// </summary>
        /// <param name="originString">要加密的明文字符串</param>
        /// <param name="encryptKey">加密密钥</param>
        /// <returns>加密后的密文</returns>
        string Encrypt(string originString, string encryptKey);
        /// <summary>
        /// 使用指定的密钥解密已经加密的密文
        /// </summary>
        /// <param name="encryptedSring">要解密的密文</param>
        /// <param name="encryptKey">加密密钥</param>
        /// <returns>解密后的明文字符串</returns>
        string Decrypt(string encryptedSring, string encryptKey);
    }
}
