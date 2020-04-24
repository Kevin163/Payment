using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GemstarPaymentCore.Data
{
    /// <summary>
    /// 支付明细扩展
    /// </summary>
    public static class UnionPayLcswDetailExtension
    {
        /// <summary>
        /// 获取聚合支付记录的支付方式
        /// 如果是没有多种支付方式的，则直接使用主单的支付方式
        /// 如果有多种支付方式的，则将明细按支付方式排序后，返回每一种支付方式的代码，金额和流水号信息
        /// </summary>
        /// <param name="details">对应的明细记录列表</param>
        /// <param name="pay">支付记录</param>
        /// <returns>支付记录的支付方式</returns>
        public static string GetPayTypeFromDetails(this List<UnionPayLcswDetail> details, UnionPayLcsw pay)
        {
            if (details == null)
            {
                return pay.PayType;
            }
            var detailsForPay = details.Where(w => w.PayId == pay.Id && w.PayStatus == WxPayInfoStatus.PaidSuccess).OrderBy(w => w.PayType).ToList();
            if (detailsForPay == null || detailsForPay.Count <= 0)
            {
                return pay.PayType;
            }
            var payType = new StringBuilder();
            var split = "";
            foreach (var detail in detailsForPay)
            {
                payType.Append(split).Append($"{detail.PayType}~{detail.PaidAmount}~{detail.PaidTransNo}");
                split = "^";
            }
            return payType.ToString();
        }
        /// <summary>
        /// 根据支付方式来获取对应的支付方式文字描述
        /// </summary>
        /// <param name="payType">支付方式</param>
        /// <returns>支付方式对应的文字描述</returns>
        public static string GetPayRemark(string payType)
        {
            if (string.IsNullOrEmpty(payType))
            {
                return string.Empty;
            }
            if (payType.Contains("^"))
            {
                //多付款方式
                var payTypeInfos = payType.Split('^');
                var result = new StringBuilder();
                var andString = "使用";
                foreach(var info in payTypeInfos)
                {
                    var infoArray = info.Split('~');
                    result.Append($"{andString}{GetPayTypeName(infoArray[0])}支付￥{infoArray[1]}元");
                    andString = "，";
                }

                result.Append("支付成功");
                return result.ToString();
            } else
            {
                //单付款方式
                return $"使用{GetPayTypeName(payType)}支付成功";
            }
        }
        public static string GetPayTypeName(string payType)
        {
            switch (payType)
            {
                case UnionPayLcswDetail.PayTypeMemberTicket:
                    return "会员优惠券";
                    break;
                case UnionPayLcswDetail.PayTypeMember:
                    return "会员卡";
                    break;
                default:
                    return $"扫呗聚合支付中的{payType}";
                    break;
            }
        }
    }
}
