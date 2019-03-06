using Essensoft.AspNetCore.Payment.Security;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GemstarPaymentCore.Business.Utility;

namespace GemstarPaymentCore.Business.BusinessHandlers.TicketsPFT
{
    /// <summary>
    /// 票付通父类，用于实现一些通用的业务处理方法
    /// </summary>
    public class PFTBase
    {

        /// <summary>
        /// POST提交数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postdata"></param>
        /// <returns></returns>
        protected async Task<string> SendPost(HttpClient client,string url, string postdata)
        {
            string result = "";
            try
            {
                result = await client.DoPostAsync(url, postdata);
            } catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取签名
        /// </summary>
        /// <returns></returns>
        protected string GetSignature(string method, string appSecret, string timestamp, string base64data)
        {
            return MD5Encrypt32(MD5Encrypt32(method + appSecret + timestamp + base64data));
        }

        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
        /// <returns></returns>
        protected string Base64Encrypt(string input)
        {
            return Convert.ToBase64String(new UTF8Encoding().GetBytes(input));
        }

        /// <summary>
        /// 32位MD5加密方法
        /// </summary>
        /// <param name="Text">待加密字串</param>
        /// <returns>加密后的字串</returns>
        protected string MD5Encrypt32(string Text)
        {
            return MD5Helper.MD5Encrypt(Text, Encoding.ASCII, MD5Helper.MD5Length.Length32);
        }


        /// <summary>  
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        protected string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
    }
}
