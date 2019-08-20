using Essensoft.AspNetCore.Payment.LcswPay.Notify;
using GemstarPaymentCore.Data;
using GemstarPaymentCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Controllers
{
    /// <summary>
    /// 扫呗聚合支付成功后的回调
    /// </summary>
    public class LcswPayNotifyController : Controller
    {
        private IWxPayDBFactory _wxPayDBFactory;
        private ILogger _logger;
        private IHttpClientFactory _httpClientFactory;
        public LcswPayNotifyController(IWxPayDBFactory wxPayDBFactory, ILogger<LcswPayNotifyController> logger,IHttpClientFactory httpClientFactory)
        {
            _wxPayDBFactory = wxPayDBFactory;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                using (var scope = _logger.BeginScope(this))
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    _logger.LogInformation($"接收到扫呗支付通知{body}");
                    var notifyRequest = JsonConvert.DeserializeObject<LcswPayNotifyRequest>(body);
                    if (notifyRequest != null)
                    {
                        notifyRequest.Body = body;
                        //这里本来就是支付成功了才通行的，所以此处不再需要检查支付状态
                        var payDb = _wxPayDBFactory.GetFirstHavePaySystemDB();
                        var terminalTime = DateTime.ParseExact(notifyRequest.TerminalTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        var payEntities = payDb.UnionPayLcsws.Where(w => w.TerminalTrace == notifyRequest.TerminalTrace && w.TerminalTime == terminalTime).ToList();
                        if (payEntities == null || payEntities.Count == 0)
                        {
                            return Json(NotifyResult.Failure("没有指定的支付记录"));
                        }
                        foreach (var payEntity in payEntities)
                        {
                            if (payEntity.Status != WxPayInfoStatus.PaidSuccess)
                            {
                                payEntity.Status = WxPayInfoStatus.PaidSuccess;
                                payEntity.Paytime = DateTime.ParseExact(notifyRequest.EndTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                payEntity.PayTransId = notifyRequest.OutTradeNo;
                                payEntity.PayType = notifyRequest.PayType;
                                payEntity.PayRemark = $"使用扫呗聚合支付中的{notifyRequest.PayType},支付方{notifyRequest.UserId},渠道流水号{notifyRequest.ChannelTradeNo}";
                            }
                            //通知回调地址支付状态
                            if (!string.IsNullOrEmpty(payEntity.CallbackUrl) && payEntity.CallbackUrl != "http://pay.gshis.net/ClientPay")
                            {
                                var paymentCallbackPara = new PaymentCallbackParameter
                                {
                                    BillId = payEntity.TerminalTrace,
                                    CallbackUrl = payEntity.CallbackUrl,
                                    PaidTime = payEntity.Paytime.Value,
                                    PaidTransId = payEntity.PayTransId,
                                    PaySuccess = payEntity.Status == WxPayInfoStatus.PaidSuccess,
                                    SystemName = payEntity.SystemName,
                                    ErrorMessage = "",
                                    PaidAmount = payEntity.TotalFee,
                                    PaidType = payEntity.PayType
                                };
                                PaymentCallback.CallbackNotify(paymentCallbackPara, _httpClientFactory);
                            }
                        }
                        await payDb.SaveChangesAsync();
                        return Json(NotifyResult.Success(""));
                    }
                    else
                    {
                        return Json(NotifyResult.Failure("解析格式出错"));
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(NotifyResult.Failure(ex.Message));
            }
        }
    }
}