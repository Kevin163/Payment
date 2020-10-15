namespace GemstarPaymentCore.Moniter
{
    /// <summary>
    /// 需要启动的进程信息
    /// </summary>
    public class ProcessItem
    {
        /// <summary>
        /// 进程名称，用于展示
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 要执行的文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 当前工作目录，为空表示取当前目录
        /// </summary>
        public string WorkingDirectory { get; set; }
        /// <summary>
        /// 命令参数
        /// </summary>
        public string Arguments { get; set; }
    }
}
