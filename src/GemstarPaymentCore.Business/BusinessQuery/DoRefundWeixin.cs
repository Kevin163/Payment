using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.WeChatPay;
using GemstarPaymentCore.Data;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;

namespace GemstarPaymentCore.Business.BusinessQuery
{
    /// <summary>
    /// 微信退款
    /// </summary>
    public class DoRefundWeixin : DoRefundBase
    {
        public override async Task<RefundResult> DoRefund(WaitRefundList record, IServiceProvider serviceProvider)
        {
            var client = serviceProvider.GetService<IWeChatPayClient>();
            var options = serviceProvider.GetService<IOptionsSnapshot<WeChatPayOptions>>().Value;

            var para = JsonConvert.DeserializeObject<RefundPara>(record.RefundPara);
            
            var request = new WeChatPayRefundRequest
            {
                OutRefundNo = para.OutRefundNo,
                TransactionId = para.TransactionId,
                SubAppId = para.SubAppId,
                SubMchId = para.SubMchId,
                TotalFee = Convert.ToInt32(Convert.ToDecimal(para.TotalFee) * 100),
                RefundFee = Convert.ToInt32(Convert.ToDecimal(para.RefundFee) * 100),
            };
            var response = await client.ExecuteAsync(request, ConfigHelper.WxPayCertificateName);
            var result = new RefundResult
            {
                RefundStatu = WaitRefundList.RefundStatu.StatuFail,
                RefundFailReason = ""
            };
            //根据返回消息进行处理
            if (response.ReturnCode == "SUCCESS")
            {
                //通信成功，则判断业务结果
                if (response.ResultCode == "SUCCESS")
                {
                    result.RefundStatu = WaitRefundList.RefundStatu.StatuSuccess;
                    return result;
                }
                else
                {
                    result.RefundStatu = response.ErrCode;
                    result.RefundFailReason = response.ErrCodeDes;
                    return result;
                }
            }
            else
            {
                result.RefundFailReason = response.ReturnMsg;
                return result;
            }
        }
        public class RefundPara
        {
            /// <summary>
            /// 子商户公众号
            /// </summary>
            public string SubAppId { get; set; }
            /// <summary>
            /// 子商户号
            /// </summary>
            public string SubMchId { get; set; }
            /// <summary>
            /// 支付流水号
            /// </summary>
            public string TransactionId { get; set; }
            /// <summary>
            /// 退款申请单号
            /// </summary>
            public string OutRefundNo { get; set; }
            /// <summary>
            /// 总支付金额
            /// </summary>
            public string TotalFee { get; set; }
            /// <summary>
            /// 退款金额
            /// </summary>
            public string RefundFee { get; set; }
            /// <summary>
            /// 退款操作员
            /// </summary>
            public string OpUserId { get; set; }
        }
    }
}
