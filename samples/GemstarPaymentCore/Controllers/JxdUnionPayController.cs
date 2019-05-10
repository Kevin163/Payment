using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
using GemstarPaymentCore.Business;
using GemstarPaymentCore.Business.BusinessQuery;
using GemstarPaymentCore.Business.MemberHandlers;
using GemstarPaymentCore.Data;
using GemstarPaymentCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Controllers
{
    public class JxdUnionPayController : Controller
    {
        private IWxPayDBFactory _dbFactory;
        private BusinessOption _businessOption;
        private IHttpClientFactory _httpClientFactory;
        private IMemberHandlerFactory _memberHandlerFactory;
        private IServiceProvider _serviceProvider;
        public JxdUnionPayController(IWxPayDBFactory wxPayDBFactory, IOptionsSnapshot<BusinessOption> businessOption, IMemberHandlerFactory memberHandlerFactory, IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
        {
            _dbFactory = wxPayDBFactory;
            _businessOption = businessOption.Value;
            _memberHandlerFactory = memberHandlerFactory;
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
        }
        #region 捷信达聚合支付首页
        /// <summary>
        /// 捷信达聚合支付首页
        /// 接收支付类型和支付记录id参数来进行支付
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IActionResult Index(string id, string type)
        {
            var model = new JxdUnionPayViewModel
            {
                IsParaOK = false,
                ErrorMessage = "参数错误"
            };
            if (type.Equals("lcsw", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrWhiteSpace(id))
            {
                var db = _dbFactory.GetFirstHavePaySystemDB();
                var idValue = Guid.Parse(id);
                var payEntity = db.UnionPayLcsws.FirstOrDefault(w => w.Id == idValue);
                if (payEntity != null)
                {
                    if (payEntity.Status == WxPayInfoStatus.NewForJxdUnionPay)
                    {
                        model.IsParaOK = true;
                        model.AppId = payEntity.AppId;
                        model.LcswPayQrcodeUrl = payEntity.LcswPayUnionQrcodeUrl;
                    }
                    else if (payEntity.Status == WxPayInfoStatus.PaidSuccess)
                    {
                        var remark = payEntity.PayRemark;
                        if (!string.IsNullOrWhiteSpace(remark))
                        {
                            remark = $"[{remark}]";
                        }
                        model.ErrorMessage = $"已经支付成功了，不需要再支付{remark}";
                    }
                    else if (payEntity.Status == WxPayInfoStatus.Cancel)
                    {
                        model.ErrorMessage = $"已经由业务系统撤销订单，不需要支付";
                    }
                }
            }
            if (model.IsParaOK)
            {
                if (IsWeixinEnviroment)
                {
                    //是微信环境
                    //没有指定公众号id，则无法获取支付人的微信openid和对应的会员身份，直接转到扫呗的聚合二维码进行支付
                    if (string.IsNullOrEmpty(model.AppId))
                    {
                        return Redirect(model.LcswPayQrcodeUrl);
                    }
                    //有指定公众号id，则构建获取openid的链接地址，转去获取openid，示例地址：https://open.weixin.qq.com/connect/oauth2/authorize?appid=APPID&redirect_uri=REDIRECT_URI&response_type=code&scope=SCOPE&state=STATE#wechat_redirect
                    var uriBase = new Uri(_businessOption.InternetUrl);
                    var wxOpenIdPath = Url.Action(nameof(WxOpenId));
                    var wxOpenIdUri = new Uri(uriBase, wxOpenIdPath);
                    var redirectPath = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={WebUtility.UrlEncode(model.AppId)}&redirect_uri={WebUtility.UrlEncode(wxOpenIdUri.AbsoluteUri)}&response_type=code&scope=snsapi_base&state={WebUtility.UrlEncode(id)}#wechat_redirect";
                    return Redirect(redirectPath);
                }
                else if (IsAlipayClientEnviroment)
                {
                    //如果是支付宝扫描的，则直接转到扫呗的聚合二维码进行支付
                    return Redirect(model.LcswPayQrcodeUrl);
                }
                else
                {
                    //如果使用的不是微信或者支付宝，则提示请使用微信或支付宝来扫描
                    model.IsParaOK = false;
                    model.ErrorMessage = "请使用微信或者支付宝来扫描后进行支付";
                }
            }
            return View(model);
        }
        /// <summary>
        /// 判断是否是微信浏览器环境
        /// </summary>
        private bool IsWeixinEnviroment => Request.Headers["User-Agent"].ToString().Contains("MicroMessenger");
        /// <summary>
        /// 判断是否是支付宝浏览器环境
        /// </summary>
        private bool IsAlipayClientEnviroment => Request.Headers["User-Agent"].ToString().Contains("AlipayClient");
        #endregion

        #region 接收获取微信openid回调
        /// <summary>
        /// 接收获取微信openid回调，根据接收到的参数换取openid，并且根据openid是否是有效会员决定是使用会员支付还是微信支付
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task<IActionResult> WxOpenId(string code, string state)
        {
            var model = new JxdUnionPayViewModel
            {
                IsParaOK = false,
                ErrorMessage = "参数错误"
            };
            try
            {

                //根据code换取对应的openId，示例地址：https://api.weixin.qq.com/sns/oauth2/access_token?appid=APPID&secret=SECRET&code=CODE&grant_type=authorization_code
                var payDb = _dbFactory.GetFirstHavePaySystemDB();
                var idValue = Guid.Parse(state);
                var payEntity = payDb.UnionPayLcsws.FirstOrDefault(w => w.Id == idValue);
                if (payEntity != null)
                {
                    if (payEntity.Status == WxPayInfoStatus.NewForJxdUnionPay)
                    {
                        model.IsParaOK = true;
                        //这里写死一个固定的openid用于在本地进行测试，真实调用时由于code肯定会有值的，所以肯定会取真实的openid
                        string openId = "oLI_KjkAEEEMOBquaFb3Rabi-czU";
                        if (!string.IsNullOrEmpty(code))
                        {
                            var openIdUrl = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={WebUtility.UrlEncode(payEntity.AppId)}&secret={WebUtility.UrlEncode(payEntity.AppSecret)}&code={WebUtility.UrlEncode(code)}&grant_type=authorization_code";
                            var httpClient = _httpClientFactory.CreateClient();
                            var resultStr = await httpClient.GetStringAsync(openIdUrl);
                            var resultObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultStr);
                            if (resultObj.ContainsKey("openid"))
                            {
                                openId = resultObj["openid"].ToString();
                            }
                            else
                            {
                                openId = "";
                            }
                        }
                        if (!string.IsNullOrEmpty(openId))
                        {
                            var memberInfos = await QueryMember(openId, payEntity.MemberType, payEntity.MemberUrl);
                            if (memberInfos != null && memberInfos.Count > 0)
                            {
                                //转到会员支付界面
                                var viewModel = new JxdUnionPayMemberViewModel
                                {
                                    OrderId = state,
                                    OrderTitle = payEntity.OrderBody,
                                    PayFee = payEntity.TotalFee,
                                    MemberId = memberInfos[0].Id,
                                    WxPayUrl = payEntity.LcswPayUnionQrcodeUrl
                                };
                                return View("Member", viewModel);
                            }
                        }
                    }
                    else if (payEntity.Status == WxPayInfoStatus.PaidSuccess)
                    {
                        var remark = payEntity.PayRemark;
                        if (!string.IsNullOrWhiteSpace(remark))
                        {
                            remark = $"[{remark}]";
                        }
                        model.ErrorMessage = $"已经支付成功了，不需要再支付{remark}";
                    }
                    else if (payEntity.Status == WxPayInfoStatus.Cancel)
                    {
                        model.ErrorMessage = $"已经由业务系统撤销订单，不需要支付";
                    }
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
            }
            if (model.IsParaOK)
            {
                //取openid失败，直接转到扫呗的聚合二维码进行支付
                return Redirect(model.LcswPayQrcodeUrl);
            }
            else
            {
                return View(nameof(Index), model);
            }
        }
        #endregion

        #region 会员卡支付
        /// <summary>
        /// 会员在页面中输入会员支付密码后继续使用会员卡支付
        /// </summary>
        /// <param name="cardPassword"></param>
        /// <param name="memberId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<IActionResult> MemberPay(string cardPassword, string memberId, string orderId)
        {
            try
            {
                var payDb = _dbFactory.GetFirstHavePaySystemDB();
                var idValue = Guid.Parse(orderId);
                var payEntity = payDb.UnionPayLcsws.FirstOrDefault(w => w.Id == idValue);
                if (payEntity == null)
                {
                    return Json(JsonResultData.Failure("参数错误,未找到支付记录"));
                }
                if (payEntity.Status == WxPayInfoStatus.Cancel)
                {
                    return Json(JsonResultData.Failure($"支付记录已经撤销，不需要继续支付"));
                }
                if (payEntity.Status == WxPayInfoStatus.PaidSuccess)
                {
                    return Json(JsonResultData.Failure($"支付记录已经支付成功，不需要继续支付"));
                }
                if (payEntity.Status == WxPayInfoStatus.NewForJxdUnionPay)
                {
                    var memberHandle = _memberHandlerFactory.GetMemberHandler(payEntity.MemberType, payEntity.MemberUrl);
                    var payPara = new MemberPaymentParameter
                    {
                        Amount = payEntity.TotalFee,
                        Id = memberId,
                        OrigBillNo = payEntity.TerminalTrace,
                        OutletCode = payEntity.OutletCode,
                        Password = cardPassword,
                        RefNo = payEntity.Id.ToString("N"),
                        Remark = payEntity.PayRemark
                    };
                    var payResult = await memberHandle.MemberPayment(payPara);
                    if (payResult.PaySuccess)
                    {
                        //更改记录的支付状态
                        payEntity.Status = WxPayInfoStatus.PaidSuccess;
                        payEntity.Paytime = DateTime.Now;
                        payEntity.PayRemark = $"使用会员卡支付成功";
                        payEntity.PayType = "Member";
                        var saveTask = payDb.SaveChangesAsync();
                        //通知回调地址支付状态
                        if (!string.IsNullOrEmpty(payEntity.CallbackUrl))
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
                        saveTask.Wait();
                        return Json(JsonResultData.Successed("会员卡支付成功"));
                    }
                    else
                    {
                        return Json(JsonResultData.Failure(payResult.Message));
                    }
                }
                return Json(JsonResultData.Failure($"支付记录状态不正确，当前状态是{payEntity.Status.ToString()}"));
            }
            catch (Exception ex)
            {
                return Json(JsonResultData.Failure(ex));
            }
        }
        /// <summary>
        /// 根据openid查询会员，并且返回会员信息
        /// </summary>
        /// <param name="openId">openid</param>
        /// <param name="memberType">对接的会员系统类型</param>
        /// <param name="memberUrl">会员接口地址</param>
        /// <returns>openid对应的会员信息</returns>

        private async Task<List<MemberInfo>> QueryMember(string openId, string memberType, string memberUrl)
        {
            if (!string.IsNullOrWhiteSpace(memberUrl))
            {
                var memberHandler = _memberHandlerFactory.GetMemberHandler(memberType, memberUrl);
                return await memberHandler.QueryMember(openId);
            }
            return null;
        }
        #endregion

        #region 支付成功通知
        /// <summary>
        /// 支付成功通知，用于线上收到支付结果后通知本地接口程序
        /// 本地接口程序负责更改对应的业务数据的支付状态
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public void PayNotify(PaymentCallbackParameter para)
        {
            try
            {
                var systemName = para.SystemName;
                var systemSetting = _businessOption.Systems.FirstOrDefault(w => w.Name == systemName);
                if (systemSetting != null && systemSetting.HavePay == 1)
                {
                    var dbOptionFactory = new DbContextOptionsBuilder<WxPayDB>();
                    dbOptionFactory.UseSqlServer(systemSetting.ConnStr);
                    using (var payDb = new WxPayDB(dbOptionFactory.Options))
                    {
                        var payEntity = payDb.WxPayInfos.FirstOrDefault(w => w.ID == para.BillId && w.Status == WxPayInfoStatus.NewForJxdUnionPay);
                        if (payEntity != null)
                        {
                            if (para.PaySuccess)
                            {
                                WxPayInfoHelper.JxdUnionPayPaidSuccess(payDb, payEntity, para.PaidTransId, para.PaidTime, para.PaidAmount, para.PaidType);
                            }
                            else
                            {
                                WxPayInfoHelper.JxdUnionPayPaidFail(payDb, payEntity, para.ErrorMessage);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }
        #endregion

        #region 查询支付结果
        public async Task<IActionResult> QueryPayResult(string terminalTrace)
        {
            var callPara = new JxdUnionPayResult();
            try
            {
                var payDb = _dbFactory.GetFirstHavePaySystemDB();
                var payEntity = payDb.UnionPayLcsws.FirstOrDefault(w => w.TerminalTrace == terminalTrace && w.Status != WxPayInfoStatus.Cancel);
                if (payEntity != null)
                {
                    //已经支付成功则直接返回
                    if (payEntity.Status == WxPayInfoStatus.PaidSuccess)
                    {
                        return PaySuccess(callPara, payEntity);
                    }
                    //如果还是新建状态，则调用扫呗支付查询接口进行查询
                    else if (payEntity.Status == WxPayInfoStatus.NewForJxdUnionPay)
                    {
                        var lcswClient = _serviceProvider.GetService<ILcswPayClient>();
                        var lcswOption = _serviceProvider.GetService<IOptionsSnapshot<LcswPayOption>>().Value;
                        var request = new LcswPayQueryRequest
                        {
                            PayType = "000",
                            ServiceId = "020",
                            MerchantNo = payEntity.MerchantNo,
                            TerminalId = payEntity.TerminalId,
                            TerminalTime = DateTime.Now.ToString("yyyyMMddHHmmss"),
                            TerminalTrace = Guid.NewGuid().ToString("N"),
                            PayTrace = payEntity.TerminalTrace,
                            PayTime = payEntity.TerminalTime.ToString("yyyyMMddHHmmss"),
                            OutTradeNo = ""
                        };
                        lcswOption.Token = payEntity.AccessToken;
                        var response = await lcswClient.ExecuteAsync(request, lcswOption);

                        if (response.IsReturnCodeSuccess && response.ResultCode == "01" && response.TradeState == "SUCCESS")
                        {
                            //支付成功，更改实体状态，并且直接返回
                            payEntity.Status = WxPayInfoStatus.PaidSuccess;
                            payEntity.Paytime = DateTime.ParseExact(response.EndTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                            payEntity.PayTransId = response.OutTradeNo;
                            payEntity.PayType = response.PayType;
                            payEntity.PayRemark = $"使用扫呗聚合支付中的{response.PayType},支付方{response.UserId},渠道流水号{response.ChannelTradeNo}";
                            await payDb.SaveChangesAsync();
                            return PaySuccess(callPara, payEntity);
                        }
                        else
                        {
                            callPara.ErrorMessage = $"{response.ReturnMsg}";
                            if (!string.IsNullOrEmpty(response.TradeState))
                            {
                                callPara.ErrorMessage += $"（支付状态:{response.TradeState}）";
                            }
                        }
                    }
                    //其他情况下，则把当前状态写到错误信息里面
                    else
                    {
                        callPara.ErrorMessage = $"未知的支付状态{payEntity.Status.ToString()}";
                    }
                }
                else
                {
                    callPara.ErrorMessage = "没有对应的支付记录";
                }
            }
            catch (Exception ex)
            {
                callPara.ErrorMessage = ex.Message;
            }
            return Json(callPara);
        }

        private IActionResult PaySuccess(JxdUnionPayResult callPara, UnionPayLcsw payEntity)
        {
            callPara.BillId = payEntity.TerminalTrace;
            callPara.PaidAmount = payEntity.TotalFee;
            callPara.PaidTime = payEntity.Paytime.Value;
            callPara.PaidTransId = payEntity.PayTransId;
            callPara.PaySuccess = true;
            callPara.SystemName = payEntity.SystemName;
            callPara.PaidRemark = payEntity.PayRemark;
            callPara.PaidType = payEntity.PayType;
            return Json(callPara);
        }
        #endregion
    }
}