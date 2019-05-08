using System.Collections.Generic;

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
        /// 只处理多少时间以内的数据,超过这个值的数据不再进行同步和处理，单位为分钟
        /// </summary>
        public int LastMinute { get; set; }
        /// <summary>
        /// 间隔多少时间查询一次是否有记录需要进行支付结果查询，单位为秒
        /// </summary>
        public int QueryInterval { get; set; }
        /// <summary>
        /// 本地接口的外网访问地址，用于外网回调通知
        /// </summary>
        public string InternetUrl { get; set; }
        /// <summary>
        /// 本地会员接口的外网访问地址，用于外网调用会员接口
        /// </summary>
        public string InternetMemberUrl { get; set; }
        /// <summary>
        /// 对接的会员版本,gsclub：老的cs会员,webgscrm：原来的bs会员,bspms：捷云会员
        /// </summary>
        public string MemberVersion { get; set; }
        /// <summary>
        /// 票付通接口中转地址
        /// </summary>
        public string PFTSwitchUrl { get; set; }
        /// <summary>
        /// 自我游接口地址
        /// </summary>
        public string ZWYAPIUrl { get; set; }
        /// <summary>
        /// 智游宝接口地址
        /// </summary>
        public string ZYBAPIUrl { get; set; }
        /// <summary>
        /// 广电运通接口地址，只需要设置到页面上一级，因为几个接口的页面地址不相同，但前面部分是相同的
        /// </summary>
        public string GRGBookingAPIUrl { get; set; }
        /// <summary>
        /// 捷信达线上支付地址，用于线下押金支付时发起真实的请求，线下接口收到后，将请求转给线上的接口
        /// </summary>
        public string JxdPaymentUrl { get; set; }
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
