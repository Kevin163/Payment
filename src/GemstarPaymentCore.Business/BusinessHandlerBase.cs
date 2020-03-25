using System;
using System.Threading.Tasks;
using GemstarPaymentCore.Business.Utility;

namespace GemstarPaymentCore.Business
{
    /// <summary>
    /// 业务处理父类
    /// </summary>
    public abstract class BusinessHandlerBase : IBusinessHandler
    {
        private const char splitChar = '|';
        private string _businessContent;
        /// <summary>
        /// 业务参数格式，由具体的业务处理类提供，以|分隔的参数
        /// </summary>
        protected abstract string contentFormat { get; }
        /// <summary>
        /// 业务参数加密下标数组，没有任何加密的，返回空数组，有加密的，返回相应的下标即可
        /// </summary>
        protected abstract int[] contentEncryptedIndexs { get; }

        public void SetBusinessContent(string businessContent)
        {
            _businessContent = businessContent;
        }

        public string Decrypt(string payStr, string encryptKey, ISecurity security)
        {
            var contentArray = payStr.Split(splitChar);
            var indexs = contentEncryptedIndexs;
            var indexCount = indexs.Length;
            for (var i = 0; i < indexCount; i++)
            {
                //因为paystr多了一个支付类型前缀，所以进行下标对应时需要加1
                var index = indexs[i] + 1;
                if (index >= contentArray.Length)
                {
                    break;
                }
                var content = contentArray[index];
                content = security.Decrypt(content, encryptKey);
                contentArray[index] = content;
            }
            return string.Join(splitChar, contentArray);
        }

        public async Task<HandleResult> HandleBusinessContentAsync()
        {
            //参数有效性检查
            if (string.IsNullOrWhiteSpace(_businessContent))
            {
                return HandleResult.Fail($"必须以格式'{contentFormat}'进行交互");
            }
            var length = contentFormat.Split(splitChar).Length;
            var infos = _businessContent.Split(splitChar);
            if (infos.Length < length)
            {
                return HandleResult.Fail($"必须以格式'{contentFormat}'进行交互");
            }
            try
            {
                var result = await DoHandleBusinessContentAsync(infos);
                return result;
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
        protected abstract Task<HandleResult> DoHandleBusinessContentAsync(string[] infos);

    }
}
