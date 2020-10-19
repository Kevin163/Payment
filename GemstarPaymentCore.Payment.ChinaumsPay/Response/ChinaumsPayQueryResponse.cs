using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GemstarPaymentCore.Payment.ChinaumsPay.Response
{
    /// <summary>
    /// 银商支付和退款结果查询
    /// </summary>
    public class ChinaumsPayQueryResponse:ChinaumsPayResponse
    {
        /// <summary>
        /// 账单号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 订单时间，格式yyyy-MM-dd
        /// </summary>
        public string BillDate { get; set; }
        /// <summary>
        /// 账单创建时间，格式yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 账单状态
        /// </summary>
        public string BillStatus { get; set; }
        /// <summary>
        /// 账单描述
        /// </summary>
        public string BillDesc { get; set; }
        /// <summary>
        /// 账单总金额
        /// </summary>
        public decimal? TotalAmount { get; set; }
        /// <summary>
        /// 账单二维码
        /// </summary>
        public string BillQRCode { get; set; }
        /// <summary>
        /// 支付信息
        /// </summary>
        public BillPaymentInfo BillPayment { get; set; }
        /// <summary>
        /// 退款信息
        /// </summary>
        public RefundBillPaymentInfo RefundBillPayment { get; set; }
        /// <summary>
        /// 借贷记标识 DEBIT_CARD（借记卡）；CREDIT_CARD（贷记卡）
        /// </summary>
        public string CardAttr { get; set; }
        /// <summary>
        /// 花呗分期数
        /// </summary>
        public string InstallmentNumber { get; set; }
        /// <summary>
        /// 查询结果中的支付信息
        /// </summary>
        public class BillPaymentInfo
        {
            /// <summary>
            /// 商户订单号
            /// </summary>
            public string MerOrderId { get; set; }
            /// <summary>
            /// 账单业务类型
            /// </summary>
            public string BillBizType { get; set; }
            /// <summary>
            /// 交易参考号
            /// </summary>
            public string PaySeqId { get; set; }
            /// <summary>
            /// 账单流水总金额
            /// </summary>
            public decimal? TotalAmount { get; set; }
            /// <summary>
            /// 实付金额
            /// </summary>
            public decimal? BuyerPayAmount { get; set; }
            /// <summary>
            /// 开票金额
            /// </summary>
            public decimal? InvoiceAmount { get; set; }
            /// <summary>
            /// 实付现金金额
            /// </summary>
            public decimal? BuyerCashPayAmt { get; set; }
            /// <summary>
            /// 折扣金额
            /// </summary>
            public decimal? DiscountAmount { get; set; }
            /// <summary>
            /// 买家ID
            /// </summary>
            public string BuyerId { get; set; }
            /// <summary>
            /// 买家用户名
            /// </summary>
            public string BuyerUsername { get; set; }
            /// <summary>
            /// 支付详情
            /// </summary>
            public string PayDetail { get; set; }
            /// <summary>
            /// 支付时间，格式yyyy-MM-dd HH:mm:ss
            /// </summary>
            public string PayTime { get; set; }
            /// <summary>
            /// 结算时间，格式yyyy-MM-dd
            /// </summary>
            public string SettleDate { get; set; }
            /// <summary>
            /// 交易状态
            /// </summary>
            public string Status { get; set; }
            /// <summary>
            /// 目标平台单号
            /// </summary>
            public string TargetOrderId { get; set; }
            /// <summary>
            /// 目标系统
            /// </summary>
            public string TargetSys { get; set; }
            /// <summary>
            /// 微信活动ID
            /// </summary>
            public string ActivityIds { get; set; }
        }
        /// <summary>
        /// 查询返回的退款信息
        /// </summary>
        public class RefundBillPaymentInfo
        {
            /// <summary>
            /// 商户退款单号
            /// </summary>
            public string MerOrderId { get; set; }
            /// <summary>
            /// 账单业务类型
            /// </summary>
            public string BillBizType { get; set; }
            /// <summary>
            /// 交易参考号
            /// </summary>
            public string PaySeqId { get; set; }
            /// <summary>
            /// 账单流水总金额
            /// </summary>
            public decimal? TotalAmount { get; set; }
            /// <summary>
            /// 实付金额
            /// </summary>
            public decimal? BuyerPayAmount { get; set; }
            /// <summary>
            /// 开票金额
            /// </summary>
            public decimal? InvoiceAmount { get; set; }
            /// <summary>
            /// 折扣金额
            /// </summary>
            public decimal? DiscountAmount { get; set; }
            /// <summary>
            /// 买家ID
            /// </summary>
            public string BuyerId { get; set; }
            /// <summary>
            /// 买家用户名
            /// </summary>
            public string BuyerUsername { get; set; }
            /// <summary>
            /// 支付时间，格式yyyy-MM-dd HH:mm:ss
            /// </summary>
            public string PayTime { get; set; }
            /// <summary>
            /// 结算时间，格式yyyy-MM-dd
            /// </summary>
            public string SettleDate { get; set; }
            /// <summary>
            /// 交易状态
            /// </summary>
            public string Status { get; set; }
            /// <summary>
            /// 目标平台单号
            /// </summary>
            public string TargetOrderId { get; set; }
            /// <summary>
            /// 买家实退金额
            /// </summary>
            public string RefundInvoiceAmount { get; set; }
            /// <summary>
            /// 目标系统
            /// </summary>
            public string TargetSys { get; set; }
        }
    }
}
