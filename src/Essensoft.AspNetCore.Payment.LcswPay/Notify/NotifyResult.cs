using Newtonsoft.Json;

namespace Essensoft.AspNetCore.Payment.LcswPay.Notify
{
    /// <summary>
    /// 利楚商务扫呗通行返回结果
    /// </summary>
    public class NotifyResult
    {
        private NotifyResult(string code,string msg)
        {
            ReturnCode = code;
            ReturnMsg = msg;
        }
        /// <summary>
        /// 响应码：01成功 ，02失败
        /// </summary>
        [JsonProperty("return_code")]
        public string ReturnCode { get; private set; }
        /// <summary>
        /// 返回信息提示，“签名失败”，“参数格式校验错误"等
        /// </summary>
        [JsonProperty("return_msg")]
        public string ReturnMsg { get;private set; }
        /// <summary>
        /// 创建成功返回结果实例
        /// </summary>
        /// <param name="msg">成功消息</param>
        /// <returns>成功的返回结果实例</returns>

        public static NotifyResult Success(string msg)
        {
            return new NotifyResult("01", msg);
        }
        /// <summary>
        /// 创建失败返回结果实例
        /// </summary>
        /// <param name="msg">失败消息</param>
        /// <returns>失败的返回结果实例</returns>
        public static NotifyResult Failure(string msg)
        {
            return new NotifyResult("02", msg);
        }
    }
}
