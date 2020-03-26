using GemstarPaymentCore.Business.Utility;
using GemstarPaymentCore.Data;
using System;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessQuery
{
    /// <summary>
    /// 执行真实退款的抽象父类
    /// </summary>
    public abstract class DoRefundBase
    {
        protected string SeriesNo { get; private set; }
        protected ISecurity Security { get; private set; }
        public static DoRefundBase GetDoRefundInstance(string payType, string seriesNo, ISecurity security)
        {
            DoRefundBase result = null;
            if (WaitRefundList.RefundPayType.PayTypeWeixin.Equals(payType, StringComparison.OrdinalIgnoreCase))
            {
                result = new DoRefundWeixin();
            }
            else if (WaitRefundList.RefundPayType.PayTypeAlipay.Equals(payType, StringComparison.OrdinalIgnoreCase))
            {
                result = new DoRefundAlipay();
            }
            else if (WaitRefundList.RefundPayType.PayTypeLcsw.Equals(payType, StringComparison.OrdinalIgnoreCase))
            {
                result = new DoRefundLcswPay();
            }
            else
            {
                throw new ApplicationException($"不支持退款的支付类型{payType}");
            }
            result.Security = security;
            result.SeriesNo = seriesNo;
            return result;
        }
        /// <summary>
        /// 执行退款
        /// </summary>
        /// <param name="record">要退款的记录</param>
        /// <param name="serviceProvider">服务提供者，用于获取其他需要的服务实例</param>
        /// <returns>退款结果</returns>
        public abstract Task<RefundResult> DoRefund(WaitRefundList record, IServiceProvider serviceProvider);
        /// <summary>
        /// 退款结果
        /// </summary>
        public class RefundResult
        {
            /// <summary>
            /// 退款状态
            /// </summary>
            public string RefundStatu { get; set; }
            /// <summary>
            /// 退款失败原因，执行成功时可以直接返回空字符串
            /// </summary>
            public string RefundFailReason { get; set; }
        }
    }
}
