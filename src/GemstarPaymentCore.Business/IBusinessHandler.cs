using System.Threading.Tasks;
using GemstarPaymentCore.Business.Utility;

namespace GemstarPaymentCore.Business
{
    /// <summary>
    /// 接收到的udp包处理接口
    /// </summary>
    public interface IBusinessHandler
    {
        /// <summary>
        /// 保存本次要处理的业务内容
        /// </summary>
        /// <param name="businessContent">业务内容</param>
        void SetBusinessContent(string businessContent);
        /// <summary>
        /// 解密支付字符串中的收款账号信息
        /// </summary>
        /// <param name="payStr">支付字符串</param>
        /// <param name="encryptKey">加密密钥</param>
        /// <param name="security">解密方法实例</param>
        /// <returns>解密后的支付字符串</returns>
        string Decrypt(string payStr, string encryptKey, ISecurity security);

        /// <summary>
        /// 处理业务内容
        /// </summary>
        /// <returns>处理结果</returns>
        Task<HandleResult> HandleBusinessContentAsync();
    }
}
