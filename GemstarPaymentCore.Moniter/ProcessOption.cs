using System.Collections.Generic;

namespace GemstarPaymentCore.Moniter
{
    /// <summary>
    /// 需要启动的进程配置
    /// </summary>
    public class ProcessOption
    {
        /// <summary>
        /// 所有需要启动的进程信息
        /// </summary>
        public List<ProcessItem> Items { get; set; }
    }
}
