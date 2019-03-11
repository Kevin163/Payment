using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GemstarPaymentCore.Data
{
    /// <summary>
    /// 微信支付中间表
    /// </summary>
    [Table("WxPayInfos")]
    public class WxPayInfo
    {
        /// <summary>
        /// 主键值，使用guid来生成
        /// </summary>
        [Key]
        public string ID { get; set; }
        /// <summary>
        /// 酒店ID
        /// </summary>
        public string HotelID { get; set; }
        /// <summary>
        /// 公众账号ID
        /// </summary>
        public string AppID { get; set; }
        /// <summary>
        /// 商户号
        /// </summary>
        public string MchID { get; set; }
        /// <summary>
        /// 商户支付密钥
        /// </summary>
        [Column("AppKey")]
        public string Key { get; set; }
        /// <summary>
        /// 商品ID（订单号）
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 商品标题
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 商品详情描述
        /// </summary>
        public string Detail { get; set; }
        /// <summary>
        /// 附加数据
        /// </summary>
        public string Attach { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalAmount { get; set; }
        /// <summary>
        /// 微支付金额
        /// </summary>
        public decimal? WxPaidAmount { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public WxPayInfoStatus? Status { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTime? BuildDate { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PayDate { get; set; }
        /// <summary>
        /// 支付失败原因
        /// </summary>
        public string ErrMsg { get; set; }
        /// <summary>
        /// 预支付交易会话标识
        /// </summary>
        public string PrePayID { get; set; }
        /// <summary>
        /// 传送标志位，默认为0，每扫描获取一次加1
        /// </summary>
        public int? TransFlag { get; set; }
    }
}
