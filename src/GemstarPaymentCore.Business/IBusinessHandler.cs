using System.Threading.Tasks;

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
        /// 处理业务内容
        /// </summary>
        /// <returns>处理结果</returns>
        Task<HandleResult> HandleBusinessContentAsync();
    }
}
