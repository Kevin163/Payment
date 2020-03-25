using GemstarPaymentCore.Business.Utility;
using Xunit;

namespace GemstarPaymentCore.Test
{
    public class SecurityViaAesTest
    {
        private const string Key = "KaiFHotel";
        private const string Origin = "明文信息";
        [Fact]
        public void EncryptTest()
        {
            var aes = new SecurityViaAes();
            var encrypted = aes.Encrypt(Origin, Key);
            var decrypted = aes.Decrypt(encrypted, Key);

            Assert.Equal(Origin, decrypted);
        }
    }
}
