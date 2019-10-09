using GemstarPaymentCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GemstarPaymentCore.Business.BusinessQuery
{
    /// <summary>
    /// 微信支付信息辅助类
    /// </summary>
    public static class WxPayInfoHelper
    {
        #region 查询支付宝订单状态
        /// <summary>
        /// 获取当前分店的支付信息中，第一条需要查询订单状态的信息，同时将此信息的上传标志加1
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <returns>支付信息实例或null（当没有需要上传的信息时）</returns>
        public static List<WxPayInfo> GetNeedQueryStatus(WxPayDB payDB,BusinessOption option)
        {
            return GetNeedQueryOrders(payDB, option, new List<WxPayInfoStatus?> { WxPayInfoStatus.New, WxPayInfoStatus.NewForYeePay });
        }

        /// <summary>
        /// 支付宝服务商查询状态，确认指定订单已经支付成功
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <param name="payInfo">已经支付成功的订单实例</param>
        /// <param name="transactionId">支付宝支付订单号</param>
        /// <param name="paidTime">支付完成时间</param>
        /// <param name="amount">支付金额</param>
        public static void AlipayPaidSuccess(WxPayDB payDB, WxPayInfo payInfo, string transactionId, DateTime paidTime, double amount)
        {
            var statuArray = new List<WxPayInfoStatus?> { WxPayInfoStatus.New, WxPayInfoStatus.NewForYeePay };
            PaidSuccess(payDB, payInfo.ID, transactionId, paidTime, Convert.ToDecimal(amount), statuArray);
        }
        /// <summary>
        /// 微信服务商查询状态，确认订单已经支付失败
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <param name="payInfo">确认支付失败的订单实例</param>
        /// <param name="errMsg">支付失败原因</param>
        public static void AlipayPaidFail(WxPayDB payDB, WxPayInfo payInfo, string errMsg)
        {
            PaidFail(payDB, payInfo.ID, new List<WxPayInfoStatus?> { WxPayInfoStatus.New, WxPayInfoStatus.NewForYeePay }, errMsg);
        }
        #endregion

        #region 查询微信服务商订单支付状态
        /// <summary>
        /// 获取当前分店的微信服务商支付信息中，第一条需要查询支付状态的信息，同时将此信息的处理标志加1
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <returns>微信支付信息实例或null（当没有需要上传的信息时）</returns>
        public static List<WxPayInfo> GetWxProviderOrderNeedStatus(WxPayDB payDB,BusinessOption option)
        {
            return GetNeedQueryOrders(payDB, option, new List<WxPayInfoStatus?> { WxPayInfoStatus.NewForWxProviderPay });
        }
        /// <summary>
        /// 微信服务商查询状态，确认指定订单已经支付成功
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <param name="payInfo">已经支付成功的订单实例</param>
        /// <param name="transactionId">微信支付订单号</param>
        /// <param name="paidTime">支付完成时间</param>
        /// <param name="amount">支付金额</param>
        public static void WxProviderPaidSuccess(WxPayDB payDB,WxPayInfo payInfo,string transactionId,DateTime paidTime,int amount)
        {
            PaidSuccess(payDB, payInfo.ID, transactionId, paidTime, Convert.ToDecimal(amount / 100.0), new List<WxPayInfoStatus?> { WxPayInfoStatus.NewForWxProviderPay });
        }
        /// <summary>
        /// 微信服务商查询状态，确认订单已经支付失败
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <param name="payInfo">确认支付失败的订单实例</param>
        /// <param name="errMsg">支付失败原因</param>
        public static void WxProviderPaidFail(WxPayDB payDB,WxPayInfo payInfo,string errMsg)
        {
            PaidFail(payDB, payInfo.ID, new List<WxPayInfoStatus?> { WxPayInfoStatus.NewForWxProviderPay }, errMsg);
        }
        #endregion

        #region 查询利楚商务扫呗支付状态
        /// <summary>
        /// 获取当前分店的利楚商务扫呗支付信息中，第一条需要查询支付状态的信息，同时将此信息的处理标志加1
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <returns>微信支付信息实例或null（当没有需要上传的信息时）</returns>
        public static List<WxPayInfo> GetLcswPayOrderNeedStatus(WxPayDB payDB, BusinessOption option)
        {
            return GetNeedQueryOrders(payDB, option, new List<WxPayInfoStatus?> { WxPayInfoStatus.NewForLcswPay });
        }
        /// <summary>
        /// 利楚商务扫呗支付查询状态，确认指定订单已经支付成功
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <param name="payInfo">已经支付成功的订单实例</param>
        /// <param name="transactionId">微信支付订单号</param>
        /// <param name="paidTime">支付完成时间</param>
        /// <param name="amount">支付金额</param>
        public static void LcswPayPaidSuccess(WxPayDB payDB, WxPayInfo payInfo, string transactionId, DateTime paidTime, int amount)
        {
            PaidSuccess(payDB, payInfo.ID, transactionId, paidTime, Convert.ToDecimal(amount / 100.0), new List<WxPayInfoStatus?> { WxPayInfoStatus.NewForLcswPay });
        }
        /// <summary>
        /// 利楚商务扫呗支付查询状态，确认订单已经支付失败
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <param name="payInfo">确认支付失败的订单实例</param>
        /// <param name="errMsg">支付失败原因</param>
        public static void LcswPayPaidFail(WxPayDB payDB, WxPayInfo payInfo, string errMsg)
        {
            PaidFail(payDB, payInfo.ID, new List<WxPayInfoStatus?> { WxPayInfoStatus.NewForLcswPay }, errMsg);
        }
        #endregion

        #region 查询捷信达聚合支付状态
        /// <summary>
        /// 获取当前分店的捷信达聚合支付信息中，第一条需要查询支付状态的信息，同时将此信息的处理标志加1
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <returns>微信支付信息实例或null（当没有需要上传的信息时）</returns>
        public static List<WxPayInfo> GetJxdUnionPayOrderNeedStatus(WxPayDB payDB, BusinessOption option)
        {
            return GetNeedQueryOrders(payDB, option, new List<WxPayInfoStatus?> { WxPayInfoStatus.NewForJxdUnionPay });
        }
        /// <summary>
        /// 捷信达支付查询状态，确认指定订单已经支付成功
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <param name="payInfoId">已经支付成功的订单id</param>
        /// <param name="transactionId">微信支付订单号</param>
        /// <param name="paidTime">支付完成时间</param>
        /// <param name="amount">支付金额</param>
        public static void JxdUnionPayPaidSuccess(WxPayDB payDB, string payInfoId, string transactionId, DateTime paidTime, decimal amount,string payType)
        {
            PaidSuccess(payDB, payInfoId, transactionId, paidTime,amount, new List<WxPayInfoStatus?> { WxPayInfoStatus.NewForJxdUnionPay },payType);
        }
        /// <summary>
        /// 捷信达聚合支付查询状态，确认订单已经支付失败
        /// </summary>
        /// <param name="payDB">当前分店的营业数据库实例</param>
        /// <param name="payInfo">确认支付失败的订单实例</param>
        /// <param name="errMsg">支付失败原因</param>
        public static void JxdUnionPayPaidFail(WxPayDB payDB, WxPayInfo payInfo, string errMsg)
        {
            PaidFail(payDB, payInfo.ID, new List<WxPayInfoStatus?> { WxPayInfoStatus.NewForJxdUnionPay }, errMsg);
        }
        #endregion

        #region 查询待退款记录，以便执行退款
        public static List<WaitRefundList> GetNotSendWaitRefunds(WxPayDB payDB)
        {
            return payDB.WaitRefundLists.Where(w => w.RefundStatus == WaitRefundList.RefundStatu.StatuNotSend).ToList();
        }
        #endregion

        #region 通用方法实现
        private static List<WxPayInfo> GetNeedQueryOrders(WxPayDB payDB, BusinessOption option,List<WxPayInfoStatus?> status)
        {
            var lastDate = DateTime.Now.AddMinutes(-option.LastMinute);
            var payInfos = payDB.WxPayInfos.Where(w => status.Contains(w.Status) && w.BuildDate >= lastDate && w.TotalAmount > 0)
                    .OrderBy(w => w.TransFlag).ThenBy(w => w.BuildDate)
                    .ToList();
            if (payInfos.Count > 0)
            {
                foreach (var payInfo in payInfos)
                {
                    payInfo.TransFlag = (payInfo.TransFlag ?? 0) + 1;
                }
                payDB.SaveChanges();
            }
            return payInfos;
        }
        private static void PaidSuccess(WxPayDB payDB, string payInfoId, string transactionId, DateTime paidTime, decimal amount,List<WxPayInfoStatus?> status,string payType = "")
        {
            var updateInfo = payDB.WxPayInfos.FirstOrDefault(w => w.ID == payInfoId && status.Contains(w.Status));
            if (updateInfo != null)
            {
                updateInfo.PrePayID = transactionId;
                updateInfo.PayDate = paidTime;
                updateInfo.WxPaidAmount = amount;
                updateInfo.Status = WxPayInfoStatus.PaidSuccess;
                updateInfo.ErrMsg = payType;
                payDB.SaveChanges();
            }
        }
        private static void PaidFail(WxPayDB payDB, string payInfoId,List<WxPayInfoStatus?> status, string errMsg)
        {
            var updateInfo = payDB.WxPayInfos.FirstOrDefault(w => w.ID == payInfoId && status.Contains(w.Status));
            if (updateInfo != null)
            {
                updateInfo.ErrMsg = errMsg;
                updateInfo.Status = WxPayInfoStatus.PaidFailure;
                payDB.SaveChanges();
            }
        }
        #endregion
    }
}
