using System.ComponentModel.DataAnnotations;

namespace GemstarPaymentCore.Models
{
    /// <summary>
    /// 利楚商务扫呗支付设备注册视图模型
    /// </summary>
    public class LcswPaySignViewModel
    {
        /// <summary>
        /// 扫呗商户号
        /// </summary>
        [Display(Name ="扫呗商户号")]
        [Required(ErrorMessage ="请输入扫呗商户号")]
        public string MerchantNo { get; set; }
        /// <summary>
        /// 扫呗终端号
        /// </summary>
        [Display(Name ="扫呗终端号")]
        [Required(ErrorMessage = "请输入扫呗终端号")]
        public string TerminalId { get; set; }
        /// <summary>
        /// 终端流水号，填写商户系统的订单号
        /// </summary>
        [Display(Name ="终端流水号")]
        [Required(ErrorMessage ="请输入终端流水号")]
        public string TerminalTrace { get; set; }
        /// <summary>
        /// 终端注册时间，yyyyMMddHHmmss，全局统一时间格式
        /// </summary>
        [Display(Name ="终端注册时间")]
        [Required(ErrorMessage ="请输入终端注册时间")]
        public string TerminalTime { get; set; }
    }
}
