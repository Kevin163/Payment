namespace GemstarPaymentCore.Data
{
    /// <summary>
    /// 支付数据库实例工厂接口
    /// </summary>
    public interface IWxPayDBFactory
    {
        /// <summary>
        /// 获取第一个设置启用了支付的业务数据库实例
        /// 检查顺序：就是按配置文件中的顺序
        /// 如果全部都没有启用，则抛出异常
        /// </summary>
        /// <returns>第一个设置启用了支付的业务数据库实例</returns>
        WxPayDB GetFirstHavePaySystemDB();
        /// <summary>
        /// 获取指定系统的支付数据库实例
        /// </summary>
        /// <param name="systemName">系统名称：CY，KF，SN，WQ，Ticket</param>
        /// <returns>对应的数据库实例
        /// 如果对应的业务系统没有启用支付，则抛出异常
        /// </returns>
        WxPayDB GetSystemDB(string systemName);
    }
}
