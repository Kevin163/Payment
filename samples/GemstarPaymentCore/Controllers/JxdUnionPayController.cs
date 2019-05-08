using GemstarPaymentCore.Business;
using GemstarPaymentCore.Business.MemberHandlers;
using GemstarPaymentCore.Data;
using GemstarPaymentCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public JxdUnionPayController(IWxPayDBFactory wxPayDBFactory,IOptionsSnapshot<BusinessOption> businessOption,IMemberHandlerFactory memberHandlerFactory)
        {
            _dbFactory = wxPayDBFactory;
            _businessOption = businessOption.Value;
            _memberHandlerFactory = memberHandlerFactory;
        }
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
                if(payEntity != null)
                {
                    if (payEntity.Status == WxPayInfoStatus.NewForLcswPay)
                    {
                        model.IsParaOK = true;
                        model.AppId = payEntity.AppId;
                        model.LcswPayQrcodeUrl = payEntity.LcswPayUnionQrcodeUrl;
                    }else if(payEntity.Status == WxPayInfoStatus.PaidSuccess)
                    {
                        var remark = payEntity.PayRemark;
                        if (!string.IsNullOrWhiteSpace(remark))
                        {
                            remark = $"[{remark}]";
                        }
                        model.ErrorMessage = $"已经支付成功了，不需要再支付{remark}";
                    }else if(payEntity.Status == WxPayInfoStatus.Cancel)
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
                    //先直接跳转到会员支付页面，以便在本地测试会员支付
                    return RedirectToAction(nameof(WxOpenId), new { state = id });
                    //有指定公众号id，则构建获取openid的链接地址，转去获取openid，示例地址：https://open.weixin.qq.com/connect/oauth2/authorize?appid=APPID&redirect_uri=REDIRECT_URI&response_type=code&scope=SCOPE&state=STATE#wechat_redirect
                    var uriBase = new Uri(_businessOption.InternetUrl);
                    var wxOpenIdPath = Url.Action(nameof(WxOpenId));
                    var wxOpenIdUri = new Uri(uriBase, wxOpenIdPath);
                    var redirectPath = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={WebUtility.UrlEncode(model.AppId)}&redirect_uri={WebUtility.UrlEncode(wxOpenIdUri.AbsoluteUri)}&response_type=code&scope=snsapi_base&state={WebUtility.UrlEncode(id)}#wechat_redirect";
                    return Redirect(redirectPath);
                }else if (IsAlipayClientEnviroment)
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
        public async Task<IActionResult> WxOpenId(string code,string state)
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
                if (payEntity != null )
                {
                    if (payEntity.Status == WxPayInfoStatus.NewForLcswPay)
                    {
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
            }catch(Exception ex)
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
                return View(nameof(Index),model);
            }
        }
        public IActionResult MemberPay(string cardPassword,string memberId,string orderId)
        {
            return Json(new JsonResultData{ Success = false, Data = $"还没有实现;cardPassword:{cardPassword};memberId:{memberId};orderId:{orderId}" });
        }
        private bool IsWeixinEnviroment => Request.Headers["User-Agent"].ToString().Contains("MicroMessenger");
        private bool IsAlipayClientEnviroment => Request.Headers["User-Agent"].ToString().Contains("AlipayClient");

        private async Task<List<MemberInfo>> QueryMember(string openId,string memberType,string memberUrl)
        {
            var memberHandler = _memberHandlerFactory.GetMemberHandler(memberType, memberUrl);
            return await memberHandler.QueryMember(openId);
        }

    }
}