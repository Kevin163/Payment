using System;
using System.Collections.Generic;
using System.Text;

namespace GemstarPaymentCore.Business
{
    /// <summary>
    /// 业务参数
    /// </summary>
    public class BusinessOption
    {
        /// <summary>
        /// 条码支付时，如果返回中间结果时，再次进行查询的超时时间，以秒为单位
        /// </summary>
        public int BarcodePayTimeout { get; set; }
    }
}
