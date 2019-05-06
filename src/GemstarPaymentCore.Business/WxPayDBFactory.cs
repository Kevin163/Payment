using GemstarPaymentCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace GemstarPaymentCore.Business
{
    /// <summary>
    /// 数据库工厂类
    /// 根据配置文件中的业务系统是否启用支付以及对应的数据库连接来返回对应的数据库实例
    /// </summary>
    public class WxPayDBFactory : IWxPayDBFactory
    {
        private BusinessOption _businessOption;
        public WxPayDBFactory(IOptionsSnapshot<BusinessOption> businessOption)
        {
            _businessOption = businessOption.Value;
        }
        /// <summary>
        /// 获取第一个设置启用了支付的业务数据库实例
        /// 检查顺序：就是按配置文件中的顺序
        /// 如果全部都没有启用，则抛出异常
        /// </summary>
        /// <returns>第一个设置启用了支付的业务数据库实例</returns>
        public WxPayDB GetFirstHavePaySystemDB()
        {
            foreach (var system in _businessOption.Systems)
            {
                if (system.HavePay == 1)
                {
                    var dbContextOption = new DbContextOptionsBuilder<WxPayDB>();
                    dbContextOption.UseSqlServer(system.ConnStr);
                    var payDB = new WxPayDB(dbContextOption.Options);
                    return payDB;
                }
            }
            throw new ApplicationException("没有任何业务系统启用支付功能，请在设置中启用支付功能，并且配置正确的业务系统数据库连接");
        }
        /// <summary>
        /// 获取指定系统的支付数据库实例
        /// </summary>
        /// <param name="systemName">系统名称：CY，KF，SN，WQ，Ticket</param>
        /// <returns>对应的数据库实例
        /// 如果对应的业务系统没有启用支付，则抛出异常
        /// </returns>
        public WxPayDB GetSystemDB(string systemName)
        {
            var system = _businessOption.Systems.FirstOrDefault(w => w.Name == systemName);
            if (system == null)
            {
                throw new ApplicationException($"没有找到名称为{systemName}的业务系统设置");
            }
            if (system.HavePay == 1)
            {
                var dbContextOption = new DbContextOptionsBuilder<WxPayDB>();
                dbContextOption.UseSqlServer(system.ConnStr);
                var payDB = new WxPayDB(dbContextOption.Options);
                return payDB;
            }
            throw new ApplicationException($"名称为{systemName}的业务系统没有启用支付功能");
        }
    }
}
