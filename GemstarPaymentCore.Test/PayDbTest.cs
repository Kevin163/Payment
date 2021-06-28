using GemstarPaymentCore.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace GemstarPaymentCore.Test
{
    /// <summary>
    /// 支付数据库测试
    /// </summary>
    public class PayDbTest
    {
        [Fact]
        public void should_return_when_trace_and_amount_are_exists()
        {
            var dbContextOption = new DbContextOptionsBuilder<WxPayDB>();
            dbContextOption.UseSqlServer("data source=39.108.148.127,1544;initial catalog=Payment;user id=jxdpmsuser;password=pmsUser@201811;MultipleActiveResultSets=True;App=paymentCore");
            var payDB = new WxPayDB(dbContextOption.Options);
            var existEntity = payDB.UnionPayLcsws.FirstOrDefault(w => w.TerminalTrace == "8018242D06291J2Q2W4A33041Q662246" && w.Status != WxPayInfoStatus.Cancel && w.TotalFee == 320.80m);
            Assert.NotNull(existEntity);
        }
        [Fact]
        public void should_not_return_when_create_time_less_than_two_hours_ago()
        {
            var dbContextOption = new DbContextOptionsBuilder<WxPayDB>();
            dbContextOption.UseSqlServer("data source=39.108.148.127,1544;initial catalog=Payment;user id=jxdpmsuser;password=pmsUser@201811;MultipleActiveResultSets=True;App=paymentCore");
            var payDB = new WxPayDB(dbContextOption.Options);
            var minCreatedDate = DateTime.Now.AddHours(-2);
            var existEntity = payDB.UnionPayLcsws.FirstOrDefault(w => w.TerminalTrace == "8018242D06291J2Q2W4A33041Q662246" && w.Status != WxPayInfoStatus.Cancel && w.TotalFee == 320.80m && w.TerminalTime > minCreatedDate);
            Assert.Null(existEntity);
        }
    }
}
