using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.LcswPay.Notify;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
using GemstarPaymentCore.Data;
using GemstarPaymentCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
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
        private IServiceProvider _serviceProvider;
        public LcswPayNotifyController(IWxPayDBFactory wxPayDBFactory, ILogger<LcswPayNotifyController> logger,IHttpClientFactory httpClientFactory,IServiceProvider serviceProvider)
        {
            _wxPayDBFactory = wxPayDBFactory;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
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
                            //不存在时，则可能是针对明细记录进行的支付，需要检查明细记录
                            var detailId = Guid.Parse(notifyRequest.TerminalTrace);
                            var lcswDetail = payDb.UnionPayLcswDetails.FirstOrDefault(w => w.DetailId == detailId);
                            if (lcswDetail != null)
                            {
                                var allDetails = payDb.UnionPayLcswDetails.Where(w => w.PayId == lcswDetail.PayId).ToList();
                                var payEntity = payDb.UnionPayLcsws.First(w => w.Id == lcswDetail.PayId);
                                if(lcswDetail.PayStatus != WxPayInfoStatus.PaidSuccess)
                                {
                                    lcswDetail.PayStatus = WxPayInfoStatus.PaidSuccess;
                                    lcswDetail.PaidAmount = lcswDetail.Amount;
                                    lcswDetail.PaidTime = DateTime.ParseExact(notifyRequest.EndTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                    lcswDetail.PaidTransNo = notifyRequest.OutTradeNo;
                                    lcswDetail.PayType = notifyRequest.PayType;

                                    SetPayEntityPaidSuccess(notifyRequest, payEntity);
                                    payEntity.PayType = allDetails.GetPayTypeFromDetails(payEntity);

                                    await payDb.SaveChangesAsync();
                                } else if(lcswDetail.PayStatus == WxPayInfoStatus.PaidSuccess && !lcswDetail.PaidTransNo.Equals(notifyRequest.OutTradeNo))
                                {
                                    await DoRefund(payEntity, notifyRequest, lcswDetail.PaidAmount ?? lcswDetail.Amount);
                                }
                                return Json(NotifyResult.Success(""));

                            } else
                            {
                                return Json(NotifyResult.Failure("没有指定的支付记录"));
                            }
                        }
                        foreach (var payEntity in payEntities)
                        {
                            if (payEntity.Status != WxPayInfoStatus.PaidSuccess)
                            {
                                SetPayEntityPaidSuccess(notifyRequest, payEntity);
                                await payDb.SaveChangesAsync();
                            } else if(payEntity.Status == WxPayInfoStatus.PaidSuccess)
                            {
                                //已经支付成功，再次接收到支付通知的话，检查是否是同一个支付记录，不是的话，则自动退款
                                if (!payEntity.PayTransId.Equals(notifyRequest.OutTradeNo))
                                {
                                    await DoRefund(payEntity,notifyRequest, Convert.ToDecimal(payEntity.TotalFee));
                                    continue;
                                }
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

        private static void SetPayEntityPaidSuccess(LcswPayNotifyRequest notifyRequest, UnionPayLcsw payEntity)
        {
            payEntity.Status = WxPayInfoStatus.PaidSuccess;
            payEntity.Paytime = DateTime.ParseExact(notifyRequest.EndTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            payEntity.PayTransId = notifyRequest.OutTradeNo;
            payEntity.PayType = notifyRequest.PayType;
            payEntity.PayRemark = UnionPayLcswDetailExtension.GetPayRemark(notifyRequest.PayType);
        }

        private async Task DoRefund(UnionPayLcsw payEntity, LcswPayNotifyRequest notifyRequest,decimal refundAmount)
        {
            _logger.LogError($"收到重复支付通知，开始自动退款：原支付记录id:{payEntity.Id.ToString()},扫呗唯一订单号：{notifyRequest.OutTradeNo}");
            //调用退款申请接口
            var request = new LcswPayRefundRequest
            {
                PayType = notifyRequest.PayType,
                ServiceId = "030",
                MerchantNo = payEntity.MerchantNo,
                TerminalId = payEntity.TerminalId,
                TerminalTime = DateTime.Now.ToString("yyyyMMddHHmmss"),
                TerminalTrace = Guid.NewGuid().ToString("N"),
                RefundFee = Convert.ToInt32(refundAmount * 100).ToString(),
                OutTradeNo = notifyRequest.OutTradeNo,
                PayTrace = notifyRequest.TerminalTrace,
                PayTime = notifyRequest.TerminalTime,
                AuthCode = ""
            };
            var _options = _serviceProvider.GetService<IOptionsSnapshot<LcswPayOption>>().Value;
            _options.Token = payEntity.AccessToken;
            var _client = _serviceProvider.GetService<ILcswPayClient>();
            var response = await _client.ExecuteAsync(request, _options);
            _logger.LogError($"收到的退款结果：{response.Body}");
        }
    }
}