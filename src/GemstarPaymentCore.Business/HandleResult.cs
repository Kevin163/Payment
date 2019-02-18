using System;

namespace GemstarPaymentCore.Business
{
    /// <summary>
    /// Udp处理后的结果
    /// </summary>
    public class HandleResult
    {
        protected HandleResult(bool success, string msg)
        {
            Result = success ? 0 : 1;
            Content = msg;
        }
        /// <summary>
        /// 处理后的是否成功标志位，0表示成功，1表示失败
        /// </summary>
        public int Result { get; set; }
        /// <summary>
        /// 具体内容,Result为0时表示失败原因，Result为1时表示成功后的结果
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 处理结果字符串，其中包含成功标志位和具体内容
        /// </summary>
        public string ResultStr
        {
            get { return string.Format("{0}{1}", Result, Content); }
        }
        /// <summary>
        /// 创建一个处理失败的实例
        /// </summary>
        /// <param name="errMsg">失败原因</param>
        /// <returns>处理失败的实例</returns>
        public static HandleResult Fail(string errMsg)
        {
            return new HandleResult(false, errMsg);
        }
        /// <summary>
        /// 创建一个处理失败的实例
        /// </summary>
        /// <param name="ex">失败异常</param>
        /// <returns>处理失败的实例</returns>
        public static HandleResult Fail(Exception ex)
        {
            var inner = ex;
            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }
            return new HandleResult(false, "接口返回：" + inner.Message);
        }
        /// <summary>
        /// 创建一个处理成功的实例
        /// </summary>
        /// <param name="data">成功后要返回的内容</param>
        /// <returns>处理成功的实例</returns>
        public static HandleResult Success(string data)
        {
            return new HandleResult(true, data);
        }
    }
}
