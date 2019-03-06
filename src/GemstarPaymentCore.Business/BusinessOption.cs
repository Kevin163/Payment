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
        /// <summary>
        /// 票付通接口中转地址
        /// </summary>
        public string PFTSwitchUrl { get; set; }
        /// <summary>
        /// 自我游接口地址
        /// </summary>
        public string ZWYAPIUrl { get; set; }
        /// <summary>
        /// 业务系统信息
        /// </summary>
        public List<BusinessSystemInfo> Systems { get; set; }
    }
    /// <summary>
    /// 业务系统信息
    /// </summary>
    public class BusinessSystemInfo
    {
        /// <summary>
        /// 业务系统名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 是否接入支付,1为接入，0为未接入
        /// </summary>
        public byte HavePay { get; set; }
        /// <summary>
        /// 是否需要查询，1为需要查询，0为不需要。如果使用了打印二维码支付的话，则需要查询
        /// </summary>
        public byte NeedQuery { get; set; }
        /// <summary>
        /// 业务数据库连接字符串
        /// 只有在接入支付时才会起作用
        /// </summary>
        public string ConnStr { get; set; }
    }
}
