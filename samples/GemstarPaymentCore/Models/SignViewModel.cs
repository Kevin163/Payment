using System.ComponentModel.DataAnnotations;

namespace GemstarPaymentCore.Models
{
    /// <summary>
    /// 加密视图模型
    /// </summary>
    public class SignViewModel
    {
        /// <summary>
        /// 加密内容
        /// </summary>
        [Required(ErrorMessage = "请输入加密内容")]
        [Display(Name ="加密内容")]
        public string Content { get; set; }
        /// <summary>
        /// 加密密钥
        /// </summary>
        [Required(ErrorMessage = "请输入加密密钥")]
        [Display(Name ="加密密钥")]
        public string Key { get; set; }
    }
}
