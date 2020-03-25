using System;
using System.Security.Cryptography;
using System.Text;

namespace GemstarPaymentCore.Business.Utility
{
    /// <summary>
    /// 加密解密辅助类
    /// </summary>
    public class SecurityViaAes : ISecurity
    {
        //默认密钥向量
        private static readonly string _ivString = "Gemstar598";
        private static readonly string _encryptKey = "GSJxd598";
        public string Decrypt(string encryptedString, string encryptKey)
        {
            if (string.IsNullOrEmpty(encryptedString))
            {
                throw new ArgumentNullException(nameof(encryptedString));
            }
            if (string.IsNullOrEmpty(encryptKey))
            {
                throw new ArgumentNullException(nameof(encryptKey));
            }
            var input = Convert.FromBase64String(encryptedString);
            using (var aes = Aes.Create())
            {
                var rgbKeys = GetEncryptedKeyBytes(encryptKey,aes.LegalKeySizes);
                var rgbIv = GetEncryptedKeyBytes(_ivString, aes.LegalBlockSizes);
                using (var cs = aes.CreateDecryptor(rgbKeys, rgbIv))
                {
                    var decrypted = cs.TransformFinalBlock(input, 0, input.Length);
                    return Encoding.UTF8.GetString(decrypted);
                }
            }

        }

        public string Encrypt(string originString, string encryptKey)
        {
            if (string.IsNullOrEmpty(originString))
            {
                throw new ArgumentNullException(nameof(originString));
            }
            if (string.IsNullOrEmpty(encryptKey))
            {
                throw new ArgumentNullException(nameof(encryptKey));
            }
            var input = Encoding.UTF8.GetBytes(originString);
            using (var aes = Aes.Create())
            {
                var rgbKeys = GetEncryptedKeyBytes(encryptKey,aes.LegalKeySizes);
                var rgbIv = GetEncryptedKeyBytes(_ivString, aes.LegalBlockSizes);
                using (var cs = aes.CreateEncryptor(rgbKeys, rgbIv))
                {
                    var encrypted = cs.TransformFinalBlock(input, 0, input.Length);
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        private static byte[] GetEncryptedKeyBytes(string encryptKey,KeySizes[] keySizes)
        {
            encryptKey = $"{encryptKey}{_encryptKey}";
            byte[] rgbKeys = Encoding.UTF8.GetBytes(encryptKey);
            var result = new byte[keySizes[0].MinSize / 8];
            Array.Copy(rgbKeys, result, result.Length);
            return result;
        }
    }
}
