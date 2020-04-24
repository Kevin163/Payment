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
                    //增加有效期功能，限制打单后两小时内进行支付，超出时间的则直接提示超时有效期
                    var validDate = DateTime.Now.AddHours(-2);
                    if (payEntity.Status == WxPayInfoStatus.NewForJxdUnionPay && payEntity.TerminalTime >= validDate)
                    {
                        model.IsParaOK = true;
                        model.AppId = payEntity.AppId;
                        model.LcswPayQrcodeUrl = payEntity.LcswPayUnionQrcodeUrl;
                        //如果仍然是待支付状态的，则需要检查一下是否有支付明细。有可能是使用优惠券支付后，后续的支付过程没有完成，再次进来的，如果是的话，则只需要继续完成支付即可
                        var lcswDetail = db.UnionPayLcswDetails.FirstOrDefault(w => w.PayId == payEntity.Id && w.PayStatus == WxPayInfoStatus.NewForJxdUnionPay && w.PayType == UnionPayLcswDetail.PayTypeLcsw);
                        if(lcswDetail != null && !string.IsNullOrEmpty(lcswDetail.PayQrcodeUrl))
                        {
                            model.LcswPayQrcodeUrl = lcswDetail.PayQrcodeUrl;
                        }
                    }
                    else if (payEntity.Status == WxPayInfoStatus.PaidSuccess)
                    {
                        var successModel = new JxdUnionPaySuccessViewModel
                        {
                            PaidAmount = payEntity.TotalFee.ToString("0.00"),
                            PaidMethod = payEntity.PayRemark
                        };
                        return View("Success", successModel);
                    }else if (payEntity.Status == WxPayInfoStatus.Cancel)
                    {
                        model.ErrorMessage = $"已经由业务系统撤销订单，不需要支付";
                    }else if(payEntity.TerminalTime < validDate)
                    {
                        model.ErrorMessage = "链接已经失效，只能支付2小时内的单据，请通知服务员重新打单";
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
                        model.LcswPayQrcodeUrl = payEntity.LcswPayUnionQrcodeUrl;
                        //这里写死一个固定的openid用于在本地进行测试，真实调用时由于code肯定会有值的，所以肯定会取真实的openid
                        string openId = "oavrb5jZ6ayJ5wgtecrv6zzix5sM";
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
                            var memberInfos = await QueryMember(openId, payEntity.MemberType, payEntity.MemberUrl,payEntity.MemberPara);
                            if (memberInfos != null && memberInfos.Count > 0)
                            {
                                var memberInfosDisplay = memberInfos.Select(w => new MemberInfoDisplay(w)).ToList();
                                var memberAllIds = string.Join(',', memberInfos.Select(w => w.Id).ToArray());
                                //转到会员支付界面
                                var viewModel = new JxdUnionPayMemberViewModel
                                {
                                    OrderId = state,
                                    OrderTitle = payEntity.OrderBody,
                                    PayFee = payEntity.TotalFee,
                                    WxPayUrl = payEntity.LcswPayUnionQrcodeUrl,
                                    MemberId = memberInfos[0].Id,
                                    MemberInfos = memberInfosDisplay,
                                    MemberAllIds = memberAllIds,
                                    UsedTicket = false,
                                    TicketAmount = 0,
                                    TicketNo = ""
                                };
                                //需要先检查支付明细中，是否已经有使用过优惠券明细，有的话，则需要返回优惠券的相关信息，以便界面上进行控制，并且不允许再使用其他优惠券进行支付，一张单先只允许使用一张优惠券
                                var ticketDetail = payDb.UnionPayLcswDetails.FirstOrDefault(w => w.PayId == payEntity.Id && w.PayStatus == WxPayInfoStatus.PaidSuccess && w.PayType == UnionPayLcswDetail.PayTypeMemberTicket);
                                if(ticketDetail != null)
                                {
                                    viewModel.UsedTicket = true;
                                    viewModel.TicketNo = ticketDetail.PaidTransNo;
                                    viewModel.TicketAmount = ticketDetail.PaidAmount;
                                }
                                return View("Member", viewModel);
                            }
                            else
                            {
                                //还不是会员，如果有设置会员绑定页面地址，则转到非会员支付页面，让客人选择是进行绑定还是直接微信支付
                                if (!string.IsNullOrEmpty(payEntity.MemberBindUrl))
                                {
                                    var viewModel = new JxdUnionPayGuestViewModel
                                    {
                                        MemberBindUrl = payEntity.MemberBindUrl,
                                        OrderId = state,
                                        OrderTitle = payEntity.OrderBody,
                                        PayFee = payEntity.TotalFee,
                                        WxPayUrl = payEntity.LcswPayUnionQrcodeUrl
                                    };
                                    return View("Guest", viewModel);

                                }
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
                model.IsParaOK = false;
                model.ErrorMessage = ex.Message;
            }
            if (model.IsParaOK && !string.IsNullOrEmpty(model.LcswPayQrcodeUrl))
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

        #region 查询会员优惠券
        public async Task<ActionResult> QueryMemberTicket(Guid id,string profileIds)
        {
            try
            {
                var payDb = _dbFactory.GetFirstHavePaySystemDB();
                var payEntity = payDb.UnionPayLcsws.FirstOrDefault(w => w.Id == id);
                if (payEntity != null)
                {
                    if (!string.IsNullOrWhiteSpace(payEntity.MemberUrl))
                    {
                        var memberHandler = _memberHandlerFactory.GetMemberHandler(payEntity.MemberType, payEntity.MemberUrl, payEntity.MemberPara);
                        var memberTickets = await memberHandler.QueryMemberTickets(new MemberTicketQueryParameter { Id = profileIds,OutletCode = payEntity.OutletCode});
                        if(memberTickets != null && memberTickets.Count > 0)
                        {
                            return Json(JsonResultData.Successed(memberTickets));
                        } 
                    }
                }
                return Json(JsonResultData.Failure("没有可用的优惠券"));

            } catch(Exception ex)
            {
                return Json(JsonResultData.Failure(ex));
            }
        }
        #endregion

        #region 使用会员优惠券
        public async Task<ActionResult> UseMemberTicket(Guid id,string profileId,string ticketId,decimal ticketAmount)
        {
            try
            {
                var payDb = _dbFactory.GetFirstHavePaySystemDB();
                var payEntity = payDb.UnionPayLcsws.FirstOrDefault(w => w.Id == id);
                if (payEntity != null)
                {
                    var result = new JxdUnionPayTicketUseResultViewModel
                    {
                        TicketUseSuccess = false,
                        TicketAmount = 0,
                        Msg = "会员优惠券使用失败，请继续支付",
                        NeedPay = true,
                        NeedPayAmount = payEntity.TotalFee,
                        NeedPayUrl = payEntity.LcswPayUnionQrcodeUrl
                    };
                    //为避免重复，需要检查是否已经有券的使用明细，有则表示是重复提交，不再真实核销券，而是直接返回之前的数据
                    var allDetails = payDb.UnionPayLcswDetails.Where(w => w.PayId == payEntity.Id && w.PayStatus == WxPayInfoStatus.PaidSuccess).ToArray();
                    if (allDetails != null && allDetails.Length > 0)
                    {
                        var ticketDetail = allDetails.FirstOrDefault(w => w.PayType == UnionPayLcswDetail.PayTypeMemberTicket);
                        if (ticketDetail != null)
                        {
                            result.TicketUseSuccess = true;
                            result.TicketAmount = ticketDetail.Amount;
                            if (payEntity.TotalFee <= ticketDetail.Amount)
                            {
                                result.Msg = "会员优惠券已经使用成功";
                                result.NeedPay = false;
                            } else
                            {
                                var lcswDetail = payDb.UnionPayLcswDetails.FirstOrDefault(w => w.DetailId != ticketDetail.DetailId && w.PayId == payEntity.Id);
                                if (lcswDetail != null)
                                {
                                    if (lcswDetail.PayStatus == WxPayInfoStatus.NewForJxdUnionPay)
                                    {
                                        result.NeedPay = true;
                                        result.NeedPayAmount = lcswDetail.Amount;
                                        result.NeedPayUrl = lcswDetail.PayQrcodeUrl;
                                    } else if(lcswDetail.PayStatus == WxPayInfoStatus.PaidSuccess)
                                    {
                                        result.NeedPay = false;
                                        result.Msg = $"已经使用扫呗{lcswDetail.PayType}支付成功";
                                    }
                                }
                            }
                            return Json(JsonResultData.Successed(result));
                        }
                    }
                    var memberHandler = _memberHandlerFactory.GetMemberHandler(payEntity.MemberType, payEntity.MemberUrl, payEntity.MemberPara);
                    var useTicketResult = await memberHandler.MemberUseTicket(new MemberTicketUseParameter { Id = profileId, Operator = "JxdUnionPay", OutletCode = payEntity.OutletCode, Remark = "聚合支付时使用优惠券", TicketCode = ticketId,RefNo = payEntity.TerminalTrace });
                    if (useTicketResult.PaySuccess)
                    {
                        result.TicketUseSuccess = true;
                        result.TicketAmount = ticketAmount;
                        result.Msg = "会员优惠券使用成功";
                        //增加优惠券使用记录
                        var ticketDetail = new UnionPayLcswDetail
                        {
                            DetailId = Guid.NewGuid(),
                            PayId = payEntity.Id,
                            CDate = DateTime.Now,
                            Amount = ticketAmount,
                            PaidAmount = ticketAmount,
                            PaidTime = DateTime.Now,
                            PaidTransNo = ticketId,
                            PayQrcodeUrl = "",
                            PayStatus = WxPayInfoStatus.PaidSuccess,
                            PayType = UnionPayLcswDetail.PayTypeMemberTicket
                        };
                        payDb.UnionPayLcswDetails.Add(ticketDetail);
                        var balance = payEntity.TotalFee - ticketAmount;
                        //将使用券后的剩余金额重新生成一条支付记录
                        if(balance > 0)
                        {
                            var balanceDetail = new UnionPayLcswDetail
                            {
                                DetailId = Guid.NewGuid(),
                                PayId = payEntity.Id,
                                CDate = DateTime.Now,
                                Amount = balance,
                                PayStatus = WxPayInfoStatus.NewForJxdUnionPay,
                                PayType = UnionPayLcswDetail.PayTypeLcsw
                            };
                            //调用扫呗接口，重新下单生成支付二维码
                            //计算支付成功后的回调通知路径
                            var notifyUri = "";
                            if (!string.IsNullOrEmpty(_businessOption.InternetUrl))
                            {
                                var uriBase = new Uri(_businessOption.InternetUrl);
                                var notifyPath = $"/LcswPayNotify";
                                notifyUri = new Uri(uriBase, notifyPath).AbsoluteUri;
                            }
                            //调用扫码支付接口
                            var request = new LcswPayUnionQrcodePayRequest
                            {
                                MerchantNo = payEntity.MerchantNo,
                                TerminalId = payEntity.TerminalId,
                                TerminalTime = balanceDetail.CDate.ToString("yyyyMMddHHmmss"),
                                TerminalTrace = balanceDetail.DetailId.ToString("N"),
                                TotalFee = Convert.ToInt32(Convert.ToDecimal(balance) * 100).ToString(),
                                OrderBody = payEntity.OrderBody,
                                Attach = payEntity.Attach,
                                NotifyUrl = notifyUri
                            };
                            var _options = _serviceProvider.GetService<IOptions<LcswPayOption>>().Value;
                            _options.Token = payEntity.AccessToken;
                            var _client = _serviceProvider.GetService<ILcswPayClient>();
                            var response = await _client.ExecuteAsync(request, _options);

                            if (!response.IsReturnCodeSuccess)
                            {
                                return Json(JsonResultData.Failure(response.ReturnMsg));
                            }
                            if (response.ResultCode != "01")
                            {
                                return Json(JsonResultData.Failure($"错误代码{response.ResultCode};错误描述:{response.ReturnMsg}"));
                            }
                            var resultStr = $"{response.QrCode}";
                            balanceDetail.PayQrcodeUrl = resultStr;
                            payDb.UnionPayLcswDetails.Add(balanceDetail);

                            result.Msg = $"会员优惠券已抵扣{ticketAmount}元，剩余{balance}元请继续支付";
                            result.NeedPay = true;
                            result.NeedPayAmount = balance;
                            result.NeedPayUrl = resultStr;
                        } else
                        {
                            //券的金额可全部抵扣支付金额，则券使用成功则表示整张单支付成功
                            payEntity.Status = WxPayInfoStatus.PaidSuccess;
                            payEntity.Paytime = DateTime.Now;
                            payEntity.PayRemark = UnionPayLcswDetailExtension.GetPayRemark(UnionPayLcswDetail.PayTypeMemberTicket);
                            payEntity.PayType = UnionPayLcswDetail.PayTypeMemberTicket;
                            payEntity.PayTransId = ticketId;
                            result.Msg = "会员优惠券已全额支付，无需继续支付";
                            result.NeedPay = false;
                        }
                        await payDb.SaveChangesAsync();
                        return Json(JsonResultData.Successed(result));
                    }
                    return Json(JsonResultData.Failure(useTicketResult.Message));
                }
                return Json(JsonResultData.Failure("优惠券使用失败"));
            }catch(Exception ex)
            {
                return Json(JsonResultData.Failure(ex));
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
        public async Task<IActionResult> MemberPay(string cardPassword, string memberId, string orderId,string cardNo)
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
                var validDate = DateTime.Now.AddHours(-2);
                if (payEntity.TerminalTime < validDate)
                {
                    return Json(JsonResultData.Failure("链接已经失效，只能支付2小时内的单据，请通知服务员重新打单"));
                }
                if (payEntity.Status == WxPayInfoStatus.NewForJxdUnionPay)
                {
                    var payAmount = payEntity.TotalFee;
                    //如果支付记录明细里面有相应的明细记录，则取出其中的待支付记录
                    var allDetails = payDb.UnionPayLcswDetails.Where(w => w.PayId == payEntity.Id).ToList();
                    var needPayDetail = allDetails?.FirstOrDefault(w => w.PayId == payEntity.Id && w.PayStatus == WxPayInfoStatus.NewForJxdUnionPay);
                    if(needPayDetail != null)
                    {
                        payAmount = needPayDetail.Amount;
                    }
                    var memberHandle = _memberHandlerFactory.GetMemberHandler(payEntity.MemberType, payEntity.MemberUrl,payEntity.MemberPara);
                    var payPara = new MemberPaymentParameter
                    {
                        Amount = payAmount,
                        Id = memberId,
                        OrigBillNo = payEntity.Id.ToString("N"),
                        OutletCode = payEntity.OutletCode,
                        Password = cardPassword,
                        RefNo = payEntity.TerminalTrace,
                        Remark = payEntity.PayRemark
                    };
                    var payResult = await memberHandle.MemberPayment(payPara);
                    if (payResult.PaySuccess)
                    {
                        //更改记录的支付状态
                        payEntity.Status = WxPayInfoStatus.PaidSuccess;
                        payEntity.Paytime = DateTime.Now;
                        payEntity.PayRemark = UnionPayLcswDetailExtension.GetPayRemark(UnionPayLcswDetail.PayTypeMember);
                        payEntity.PayType = UnionPayLcswDetail.PayTypeMember;
                        payEntity.PayTransId = cardNo;
                        if(needPayDetail != null)
                        {
                            needPayDetail.PaidAmount = payAmount;
                            needPayDetail.PaidTime = DateTime.Now;
                            needPayDetail.PaidTransNo = payPara.OrigBillNo;
                            needPayDetail.PayStatus = WxPayInfoStatus.PaidSuccess;
                            needPayDetail.PayType = "Member";

                            payEntity.PayType = allDetails.GetPayTypeFromDetails(payEntity);
                            payEntity.PayRemark = UnionPayLcswDetailExtension.GetPayRemark(payEntity.PayType);
                        }
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
                        return Json(JsonResultData.Successed(Url.Action(nameof(Index),new { type="lcsw",id=payEntity.Id.ToString("N")})));
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

        private async Task<List<MemberInfo>> QueryMember(string openId, string memberType, string memberUrl,string memberPara)
        {
            if (!string.IsNullOrWhiteSpace(memberUrl))
            {
                var memberHandler = _memberHandlerFactory.GetMemberHandler(memberType, memberUrl,memberPara);
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
                                WxPayInfoHelper.JxdUnionPayPaidSuccess(payDb, payEntity.ID, para.PaidTransId, para.PaidTime, para.PaidAmount, para.PaidType);
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
                        var queryPayTrace = payEntity.TerminalTrace;
                        var queryPayTime = payEntity.TerminalTime;
                        var allDetails = payDb.UnionPayLcswDetails.Where(w => w.PayId == payEntity.Id).ToList();
                        var lcswPayDetail = allDetails.FirstOrDefault(w => w.PayType == "lcsw");
                        if(lcswPayDetail != null)
                        {
                            queryPayTrace = lcswPayDetail.TerminalTrace;
                            queryPayTime = lcswPayDetail.CDate;
                        }
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
                            PayTrace = queryPayTrace,
                            PayTime = queryPayTime.ToString("yyyyMMddHHmmss"),
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
                            payEntity.PayRemark = UnionPayLcswDetailExtension.GetPayRemark(response.PayType);
                            if(lcswPayDetail != null)
                            {
                                lcswPayDetail.PayType = response.PayType;
                                lcswPayDetail.PaidAmount = lcswPayDetail.Amount;
                                lcswPayDetail.PaidTime = DateTime.ParseExact(response.EndTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                lcswPayDetail.PaidTransNo = response.OutTradeNo;
                                lcswPayDetail.PayStatus = WxPayInfoStatus.PaidSuccess;

                                payEntity.PayType = allDetails.GetPayTypeFromDetails(payEntity);
                                payEntity.PayRemark = UnionPayLcswDetailExtension.GetPayRemark(payEntity.PayType);
                            }
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