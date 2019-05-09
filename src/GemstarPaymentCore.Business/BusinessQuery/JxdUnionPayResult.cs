using System;

namespace GemstarPaymentCore.Business.BusinessQuery
{
    public class JxdUnionPayResult
    {
        /// <summary>
        /// 原账务id
        /// </summary>
        public string BillId { get; set; }
        /// <summary>
        /// 支付是否成功
        /// </summary>
        public bool PaySuccess { get; set; }
        /// <summary>
        /// 支付失败原因
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 已支付金额
        /// </summary>
        public decimal PaidAmount { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime PaidTime { get; set; }
        /// <summary>
        /// 支付流水号
        /// </summary>
        public string PaidTransId { get; set; }
        /// <summary>
        /// 业务系统
        /// </summary>
        public string SystemName { get; set; }
        /// <summary>
        /// 支付说明
        /// </summary>
        public string PaidRemark { get; set; }
    }
}
