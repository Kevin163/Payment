using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.TicketsPFT
{
    /// <summary>
    /// 票付通订单查询接口
    /// </summary>
    public class PFTQueryOrderUdpContentHandler : PFTBase, IBusinessHandler
    {
        private ILogger _log;
        private IHttpClientFactory _clientFactory;
        private BusinessOption _businessOption;
        public PFTQueryOrderUdpContentHandler(ILogger<PFTQueryOrderUdpContentHandler> logger, IHttpClientFactory httpClientFactory, IOptionsSnapshot<BusinessOption> businessOption)
        {
            _log = logger;
            _clientFactory = httpClientFactory;
            _businessOption = businessOption.Value;
        }
        protected override string contentFormat => "pftAppId|pftSecret|pftSalerId|checkType|checkValue|systemName";
        protected override int[] contentEncryptedIndexs => new int[] { };
        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] contentInfo)
        {
            try
            {
                var switchUrl = _businessOption.PFTSwitchUrl;
                if (string.IsNullOrWhiteSpace(switchUrl))
                {
                    return HandleResult.Fail("请先在配置文件中增加PFTSwitchUrl参数设置，用于与票付通进行通信");
                }
                var appId = contentInfo[0];
                var secret = contentInfo[1];
                var salerId = contentInfo[2];
                var checkType = contentInfo[3];
                var checkValue = contentInfo[4];
                var systemName = contentInfo[5];
                if (string.IsNullOrWhiteSpace(checkValue))
                {
                    return HandleResult.Fail("请指定查询类型对应的值");
                }

                var paras = new StringBuilder();
                paras.Append("{\"query_type\":\"").Append(checkType).Append("\"")
                    .Append(",\"salerid\":\"").Append(salerId).Append("\"");
                if (checkType == "1")
                {
                    paras.Append(",\"code\":\"").Append(checkValue).Append("\"}");
                }
                else if (checkType == "2")
                {
                    paras.Append(",\"mobile\":\"").Append(checkValue).Append("\"}");
                }
                else if(checkType == "3")
                {
                    paras.Append(",\"idcard\":\"").Append(checkValue).Append("\"}");
                }else
                {
                    paras.Append(",\"ordernum\":\"").Append(checkValue).Append("\"}");
                }
                var method = "Query_Order";
                string data = paras.ToString();
                string timestamp = GetTimeStamp();
                string base64data = Base64Encrypt(data);
                string signature = GetSignature(method, secret, timestamp, base64data);
                string postdata = "{\"method\":\"" + method + "\",\"app_id\":\"" + appId + "\",\"timestamp\":\"" + timestamp + "\",\"signature\":\"" + signature + "\",\"params\":\"" + base64data + "\"}";
                _log.LogInformation("PFTQueryOrderUdpContentHandler", string.Format("发送的请求字符串:{0},未编码参数:{1}", postdata,data));
                string result="";
#if MOCK
                //如果是模拟环境，则直接返回一个固定的订单内容
                result = "{Success: true,Data: {\"code\": 200,\"msg\": \"请求参数错误\",\"data\": {\"ordernum\": \"4008697\",\"code\": \"736890\", \"status\": \"1\", \"ordername\": \"武则天\", \"ordertel\": \"15060403881\",\"orderidcard\": \"\", \"ordertime\": \"2017-04-27 16:01:56\",\"begintime\": \"2017-04-27\",\"endtime\": \"2017-04-27\",\"paystatus\": \"1\",\"tickets\": [{\"tid\": \"30130\",\"tnum\": \"5\",\"tnum_s\": \"5\",\"name\": \"测试产品0426测试产品0426A\",\"ordernum\": \"4008697\",\"status\": \"1\",\"price\": 0.01,\"totalprice\": 0.05,\"buyerid\": \"3385\",\"buyername\": \"香菇旅行社\"}, {\"tid\": \"30131\",\"tnum\": \"3\",\"tnum_s\": \"3\",\"name\": \"测试产品0426测试产品0426B\",\"ordernum\": \"4008698\",\"status\": \"1\",\"price\": 0.02,\"totalprice\": 0.06,\"buyerid\": \"3385\",\"buyername\": \"香菇旅行社\"}]}}}";
#else
                try
                {
                    using (var client = _clientFactory.CreateClient())
                    {
                        result = await SendPost(client, switchUrl, postdata);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogInformation("PFTQueryOrderUdpContentHandler", "与票付通通信时遇到错误：" + ex.Message);
                }
#endif

                _log.LogInformation("PFTQueryOrderUdpContentHandler", string.Format("接收到的返回字符串:{0}", result));
                var resultObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
                if (resultObj == null)
                {
                    return HandleResult.Fail("接收到的数据格式不能正确处理");
                }
                if (!(bool)resultObj["Success"])
                {
                    return HandleResult.Fail(resultObj["Data"].ToString());
                }
                var resultObjDataStr = resultObj["Data"].ToString();
                resultObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultObjDataStr);
                if (resultObj["code"] == null || resultObj["code"].ToString() != "200")
                {
                    return HandleResult.Fail(resultObj["msg"].ToString());
                }
                //查询订单成功
                try
                {
                    //从配置信息中检查对应的业务系统是否已经启用，并且正确配置数据库
                    var businessSystemInfo = _businessOption.Systems.FirstOrDefault(w => w.Name.Equals(systemName, StringComparison.OrdinalIgnoreCase));
                    //由于订单的内容很多，需要考虑一下看如何才能有效的返回
                    if (businessSystemInfo == null || businessSystemInfo.HavePay != 1 || string.IsNullOrEmpty(businessSystemInfo.ConnStr))
                    {
                        return HandleResult.Fail($"业务系统{systemName}未接入支付或者未正确设置数据库连接信息");
                    }
                    using (var conn = new SqlConnection(businessSystemInfo.ConnStr))
                    {
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "up_itf_pftOrderQuery_JsonHandle";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        var pXml = cmd.CreateParameter();
                        pXml.ParameterName = "@orderXml";
                        pXml.SqlDbType = System.Data.SqlDbType.Text;
                        pXml.SqlValue = resultObjDataStr;
                        cmd.Parameters.Add(pXml);

                        conn.Open();
                        var businessId = cmd.ExecuteScalar().ToString();
                        conn.Close();
                        return HandleResult.Success(businessId);
                    }
                }
                catch (Exception ex)
                {
                    return HandleResult.Fail("执行存储过程up_itf_pftOrderQuery_JsonHandle时出错，原因：" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }

    }
}
