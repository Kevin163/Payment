using System.Collections.Generic;

namespace GemstarPaymentCore.Business
{
    /// <summary>
    /// 业务参数
    /// </summary>
    public class BusinessOption
    {
        public const string SystemCSGshis = "cs";
        public const string SystemBsPms = "bs";
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
        /// 会员绑定地址，要求客户使用捷信达的微信营销系统，具体地址由微信事业部根据每个客户给出，示例：http://www.gshis.net/oauth/member/index.do?comid=5771e56bd5e1ea51db566d14
        /// </summary>
        public string MemberBindWxUrl { get; set; }
        /// <summary>
        /// 对接的会员版本,gsclub：老的cs会员,webgscrm：原来的bs会员,bspms：捷云会员
        /// </summary>
        public string MemberVersion { get; set; }
        /// <summary>
        /// 捷云会员的集团id
        /// </summary>
        public string BsPmsGrpId { get; set; }
        /// <summary>
        /// 捷云会员的渠道代码
        /// </summary>
        public string BsPmsChannelCode { get; set; }
        /// <summary>
        /// 捷云会员的渠道对应的密钥
        /// </summary>
        public string BsPmsChannelKey { get; set; }
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
        /// 部署方式
        /// </summary>
        public string Deploy { get; set; }
        /// <summary>
        /// 业务系统信息
        /// </summary>
        public List<BusinessSystemInfo> Systems { get; set; }
        /// <summary>
        /// 是否启用cs客房的rc单安卓设备电子签名功能，默认为不启用，只有在用户明确购买了安卓签名设备的情况下，才需要开启此功能,0:不开启,1:开启
        /// </summary>
        public string EnableRCSign { get; set; } = "0";
        /// <summary>
        /// 客人签名图片放置到pdf中时，离下边的距离，需要根据rc单的格式进行调整，默认为100
        /// </summary>
        public string RCSignPositionBottom { get; set; } = "100";
        /// <summary>
        /// 客人签名图片放置到pdf中时，离右边的距离，需要根据rc单的格式进行调整，默认为100
        /// </summary>
        public string RCSignPositionRight { get; set; } = "100";
        /// <summary>
        /// 电子签名功能对接的系统版本，cs:线下cs客房，bs:线上捷云系统
        /// </summary>
        public string RCSystemVersion { get; set; } = SystemCSGshis;
        /// <summary>
        /// bs pms接口地址,默认为：http://pmsnotify.gshis.com/rcsign
        /// </summary>
        public string RCSignBsPmsApiUrl { get; set; } = "http://pmsnotify.gshis.com/rcsign";
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
