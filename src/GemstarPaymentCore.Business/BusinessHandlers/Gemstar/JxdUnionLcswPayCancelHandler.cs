using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GemstarPaymentCore.Data;

namespace GemstarPaymentCore.Business.BusinessHandlers.Gemstar
{
    /// <summary>
    /// 捷信达聚合支付取消接口
    /// 只有刚打单后，还没有支付成功的才可以取消成功
    /// </summary>
    public class JxdUnionLcswPayCancelHandler : IBusinessHandler
    {
        private const string contentFormat = "terminalTrace";
        private const char splitChar = '|';
        private BusinessHandlerParameter _para;
        private BusinessOption _businessOption;
        private WxPayDB _payDb;
        private string _businessContent;
        public JxdUnionLcswPayCancelHandler(IWxPayDBFactory payDBFactory)
        {
            _payDb = payDBFactory.GetFirstHavePaySystemDB();
        }


        public void SetBusinessContent(string businessContent)
        {
            _businessContent = businessContent;
        }
        public void SetBusiessHandlerParameter(BusinessHandlerParameter para)
        {
            _para = para;
        }
        public async Task<HandleResult> HandleBusinessContentAsync()
        {
            //参数有效性检查
            if (string.IsNullOrWhiteSpace(_businessContent))
            {
                return HandleResult.Fail($"必须以格式'{contentFormat}'进行交互");
            }
            var length = contentFormat.Split(splitChar).Length;
            var infos = _businessContent.Split(splitChar);
            if (infos.Length < length)
            {
                return HandleResult.Fail($"必须以格式'{contentFormat}'进行交互");
            }
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
                    if(cancelEntities.Count <= 0)
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
