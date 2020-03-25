using Essensoft.AspNetCore.Payment.LcswPay;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
using GemstarPaymentCore.Data;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.BusinessHandlers.Gemstar
{
    /// <summary>
    /// 捷信达聚合支付,使用扫呗通道的处理类
    /// </summary>
    public class JxdUnionLcswPayHandler : BusinessHandlerBase
    {
        private readonly ILcswPayClient _client;
        private readonly LcswPayOption _options;
        private BusinessHandlerParameter _para;
        private BusinessOption _businessOption;
        private WxPayDB _payDb;
        private string _businessContent;
        public JxdUnionLcswPayHandler(ILcswPayClient client, IOptionsSnapshot<LcswPayOption> options,IOptionsSnapshot<BusinessOption> businessOption,IWxPayDBFactory payDBFactory)
        {
            _client = client;
            _options = options.Value;
            _payDb = payDBFactory.GetFirstHavePaySystemDB();
            _businessOption = businessOption.Value;
        }
        protected override string contentFormat => "merchantNo|terminalId|accessToken|terminalTrace|terminalTime|outletCode|totalFee|orderBody|attach|appId|appSecret|systemName";
        protected override int[] contentEncryptedIndexs => new int[] { 0 };

        public void SetBusiessHandlerParameter(BusinessHandlerParameter para)
        {
            _para = para;
        }
        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                var merchantNo = infos[i++];
                var terminalId = infos[i++];
                var accessToken = infos[i++];
                var terminalTrace = infos[i++];
                var terminalTime = infos[i++];
                var outletCode = infos[i++];
                var totalFee = infos[i++];
                var orderBody = infos[i++];
                var attach = infos[i++];
                var appId = infos[i++];
                var appSecret = infos[i++];
                var systemName = infos[i++];
                //检查参数有效性              
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
                    MerchantNo = merchantNo,
                    TerminalId = terminalId,
                    TerminalTime = terminalTime,
                    TerminalTrace = terminalTrace,
                    TotalFee = Convert.ToInt32(Convert.ToDecimal(totalFee) * 100).ToString(),
                    OrderBody = orderBody,
                    Attach = attach,
                    NotifyUrl = notifyUri
                };
                _options.Token = accessToken;
                var response = await _client.ExecuteAsync(request, _options);

                if (!response.IsReturnCodeSuccess)
                {
                    return HandleResult.Fail(response.ReturnMsg);
                }
                if (response.ResultCode != "01")
                {
                    return HandleResult.Fail($"错误代码{response.ResultCode};错误描述:{response.ReturnMsg}");
                }
                var resultStr = $"{response.QrCode}";
                //如果是转发来的请求，则需要保存订单支付信息
                if (_para.IsFromRedirect)
                {
                    //如果有相同业务单号对应的新记录，则将原来的记录撤销
                    var cancelEntities = _payDb.UnionPayLcsws.Where(w => w.TerminalTrace == terminalTrace && w.Status == WxPayInfoStatus.NewForJxdUnionPay).ToList();
                    foreach (var cancelEntity in cancelEntities)
                    {
                        cancelEntity.Status = WxPayInfoStatus.Cancel;
                        cancelEntity.PayRemark = "业务系统重新使用相同单号重新请求支付，此订单将自动撤销";
                    }
                    //增加新记录
                    var payEntity = new UnionPayLcsw
                    {
                        Id = Guid.NewGuid(),
                        SystemName = systemName,
                        AccessToken = accessToken,
                        Attach = attach,
                        CallbackUrl = _para.CallbackUrl,
                        LcswPayUnionQrcodeUrl = resultStr,
                        MemberUrl = _para.MemberUrl,
                        MemberType = _para.MemberType,
                        MerchantNo = merchantNo,
                        OutletCode = outletCode,
                        OrderBody = orderBody,
                        TerminalId = terminalId,
                        TerminalTime = DateTime.ParseExact(terminalTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
                        TerminalTrace = terminalTrace,
                        TotalFee = Convert.ToDecimal(totalFee),
                        AppId = appId,
                        AppSecret = appSecret,
                        Status = WxPayInfoStatus.NewForJxdUnionPay,
                        MemberBindUrl = _para.MemberBindUrl
                    };
                    //处理捷云会员的会员参数,以便后续会员支付时可以取到正确的参数来调用接口
                    if (MemberHandlers.MemberHandlerFactory.IsMemberBspms(_para.MemberType))
                    {
                        var bsPara = new MemberHandlers.BSPMS.BSPmsMemberPara
                        {
                            ChannelCode = _para.BspmsChannelCode,
                            ChannelKey = _para.BspmsChannelKey,
                            GrpId = _para.BspmsGrpid
                        };
                        var bsParaStr = JsonConvert.SerializeObject(bsPara);
                        payEntity.MemberPara = bsParaStr;
                    }
                    _payDb.UnionPayLcsws.Add(payEntity);
                    await _payDb.SaveChangesAsync();
                    string id = payEntity.Id.ToString("N");
                    //计算要返回的页面地址
                    var uriBase = new Uri(_businessOption.InternetUrl);
                    var uriPara = $"/JxdUnionPay?id={WebUtility.UrlEncode(id)}&type=lcsw";
                    var uri = new Uri(uriBase, uriPara);
                    return HandleResult.Success(uri.AbsoluteUri);
                }
                //如果是直接支付的，则直接返回扫呗的聚合二维码地址
                return HandleResult.Success(resultStr);
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
