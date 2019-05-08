using System;
using System.Collections.Generic;

namespace GemstarPaymentCore.Models
{
    public class PaymentCallbackParameter
    {
        /// <summary>
        /// 回调通知地址
        /// </summary>
        public string CallbackUrl { get; set; }
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
        public List<KeyValuePair<string, string>> ToKeyValuePairs=> new List<KeyValuePair<string, string>>{
            KeyValuePair.Create(nameof(BillId),BillId)
            ,KeyValuePair.Create(nameof(PaySuccess),PaySuccess?"1":"0")
            ,KeyValuePair.Create(nameof(PaidTime),PaidTime.ToString("yyyyMMddHHmmss"))
            ,KeyValuePair.Create(nameof(PaidTransId),PaidTransId)
            ,KeyValuePair.Create(nameof(SystemName),SystemName)
            ,KeyValuePair.Create(nameof(PaidAmount),PaidAmount.ToString())
            ,KeyValuePair.Create(nameof(ErrorMessage),ErrorMessage)
        };
    }
}
