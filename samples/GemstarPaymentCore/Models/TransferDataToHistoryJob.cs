using System;
using System.Linq;
using System.Threading.Tasks;
using GemstarPaymentCore.Business.BusinessQuery;
using GemstarPaymentCore.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace GemstarPaymentCore.Models
{
    /// <summary>
    /// 转移数据到历史表的任务，会调度为每天执行一次
    /// </summary>
    public class TransferDataToHistoryJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var connStr = context.MergedJobDataMap.GetString(JobParaName.ParaConnStrName);
            if (!string.IsNullOrEmpty(connStr))
            {
                var builder = new DbContextOptionsBuilder<WxPayDB>();
                builder.UseSqlServer(connStr);
                var db = new WxPayDB(builder.Options);
                var deleteDate = DateTime.Today.AddDays(-7);
                var deleteRecords = db.UnionPayLcsws.Where(w => w.TerminalTime < deleteDate).ToList();
                db.UnionPayLcsws.RemoveRange(deleteRecords);
                db.SaveChanges();
                try
                {
                    foreach (var record in deleteRecords)
                    {
                        var history = new UnionPayLcswHistory
                        {
                            AccessToken = record.AccessToken,
                            AppId = record.AppId,
                            AppSecret = record.AppSecret,
                            Attach = record.Attach,
                            CallbackUrl = record.CallbackUrl,
                            Id = record.Id,
                            LcswPayUnionQrcodeUrl = record.LcswPayUnionQrcodeUrl,
                            MemberBindUrl = record.MemberBindUrl,
                            MemberPara = record.MemberPara,
                            MemberType = record.MemberType,
                            MemberUrl = record.MemberUrl,
                            MerchantNo = record.MerchantNo,
                            OrderBody = record.OrderBody,
                            OutletCode = record.OutletCode,
                            PayRemark = record.PayRemark,
                            Paytime = record.Paytime,
                            PayTransId = record.PayTransId,
                            PayType = record.PayType,
                            Status = record.Status,
                            SystemName = record.SystemName,
                            TerminalId = record.TerminalId,
                            TerminalTime = record.TerminalTime,
                            TerminalTrace = record.TerminalTrace,
                            TotalFee = record.TotalFee,

                        };
                        db.UnionPayLcswHistories.Add(history);
                    }
                    db.SaveChanges();
                } catch
                {

                }
            }
            return Task.CompletedTask;
        }
    }
}
