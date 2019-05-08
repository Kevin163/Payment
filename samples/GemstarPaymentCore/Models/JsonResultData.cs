using System;

namespace GemstarPaymentCore.Models
{
    /// <summary>
    /// 发送结果
    /// </summary>
    public class JsonResultData
    {

        /// <summary>
        /// 处理是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 如果是失败时，则此对象表示错误原因
        /// 如果是成功时，则此对象表示应返回给客户端的数据
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 错误代码，默认为0 区分是什么类型的错误 登录超时为1
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// 返回处理成功的json实例
        /// </summary>
        /// <param name="data">要返回给客户端的数据实例</param>
        /// <returns>处理成功的json实例</returns>
        public static JsonResultData Successed(object data)
        {
            return new JsonResultData { Success = true, Data = data };
        }

        public static JsonResultData Successed()
        {
            return new JsonResultData { Success = true, Data = "" };
        }

        /// <summary>
        /// 返回处理失败的json实例
        /// </summary>
        /// <param name="msg">失败原因</param>
        /// <returns>处理失败的json实例</returns>
        public static JsonResultData Failure(string msg, int errorCode = 0)
        {
            return new JsonResultData { Success = false, Data = msg, ErrorCode = errorCode };
        }
        /// <summary>
        /// 返回处理失败的json实例
        /// </summary>
        /// <param name="data">失败时需要返回的复杂对象</param>
        /// <returns>处理失败的json实例</returns>
        public static JsonResultData Failure(object data, int errorCode = 0)
        {
            return new JsonResultData { Success = false, Data = data, ErrorCode = errorCode };
        }
        /// <summary>
        /// 返回处理失败的json实例
        /// </summary>
        /// <param name="ex">错误异常实例</param>
        /// <returns>处理失败的json实例</returns>
        public static JsonResultData Failure(Exception ex, int errorCode = 0)
        {
            var message = FriendlyMessage(ex);

            return new JsonResultData { Success = false, Data = message, ErrorCode = errorCode };
        }
        /// <summary>
        /// 返回处理失败的json实例
        /// </summary>
        /// <param name="ex">错误异常实例</param>
        /// <returns>处理失败的json实例</returns>
        public static JsonResultData Failure(Exception ex, string prefixData, int errorCode = 0)
        {
            var message = FriendlyMessage(ex);

            return new JsonResultData { Success = false, Data = prefixData + message, ErrorCode = errorCode };
        }
        /// <summary>
        /// 将异常信息转换为友好的出错信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <returns>转换为的对应的友好出错信息</returns>
        public static string FriendlyMessage(Exception ex)
        {
            Exception inner = ex;
            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }
            return inner.Message;
        }
    }
}
