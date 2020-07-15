using Microsoft.EntityFrameworkCore;


namespace GemstarPaymentCore.Data
{
    /// <summary>
    /// 微信支付数据库
    /// </summary>
    public class WxPayDB : DbContext
    {
        /// <summary>
        /// 使用配置文件中的数据库连接名称或者连接字符串来创建数据库实例
        /// </summary>
        /// <param name="options">配置文件中的数据库连接名称或者连接字符串来创建数据库实例</param>
        public WxPayDB(DbContextOptions<WxPayDB> options) : base(options)
        {
        }
        #region 表实体
        public DbSet<WxPayInfo> WxPayInfos { get; set; }
        public DbSet<UnionPayLcsw> UnionPayLcsws { get; set; }
        public DbSet<UnionPayLcswHistory> UnionPayLcswHistories { get; set; }
        public DbSet<WaitRefundList> WaitRefundLists { get; set; }
        public DbSet<CYUserInfo> CYUserInfos { get; set; }
        public DbSet<UnionPayLcswDetail> UnionPayLcswDetails { get; set; }
        #endregion
    }
}
