using System;
using System.Linq;
using System.Threading.Tasks;
using GemstarPaymentCore.Data;

namespace GemstarPaymentCore.Business.BusinessHandlers.Gemstar
{
    /// <summary>
    /// 捷信达聚合支付取消接口
    /// 只有刚打单后，还没有支付成功的才可以取消成功
    /// </summary>
    public class JxdUnionLcswPayCancelHandler : BusinessHandlerBase
    {
        private BusinessHandlerParameter _para;
        private BusinessOption _businessOption;
        private WxPayDB _payDb;
        private string _businessContent;
        public JxdUnionLcswPayCancelHandler(IWxPayDBFactory payDBFactory)
        {
            _payDb = payDBFactory.GetFirstHavePaySystemDB();
        }
        protected override string contentFormat => "terminalTrace";
        protected override int[] contentEncryptedIndexs => new int[] { };

        public void SetBusiessHandlerParameter(BusinessHandlerParameter para)
        {
            _para = para;
        }
        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                var terminalTrace = infos[i++];
                //检查参数有效性
                if (string.IsNullOrEmpty(terminalTrace))
                {
                    return HandleResult.Fail($"请指定要取消打单的已打单terminalTrace");
                }
                //如果是转发来的请求，则需要保存订单支付信息
                if (_para.IsFromRedirect)
                {
                    //如果有相同业务单号对应的新记录，则将原来的记录撤销
                    var cancelEntities = _payDb.UnionPayLcsws.Where(w => w.TerminalTrace == terminalTrace && w.Status == WxPayInfoStatus.NewForJxdUnionPay).ToList();
                    if (cancelEntities.Count <= 0)
                    {
                        return HandleResult.Fail($"指定的单号已经不是初始打单状态，不能取消");
                    }
                    foreach (var cancelEntity in cancelEntities)
                    {
                        cancelEntity.Status = WxPayInfoStatus.Cancel;
                        cancelEntity.PayRemark = "业务系统取消打单，此订单将自动撤销";
                    }
                    await _payDb.SaveChangesAsync();

                    return HandleResult.Success("业务系统取消打单成功");
                }
                //如果是直接支付的，则直接返回扫呗的聚合二维码地址
                return HandleResult.Fail($"直接支付的无法取消打单");
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
