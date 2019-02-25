using GemstarPaymentCore.Business.BusinessHandlers.LcswPay;
using GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GemstarPaymentCore.Business.BusinessHandlers
{
    /// <summary>
    /// UdpContentHandler工厂
    /// </summary>
    public static class BusinessHandlerFactory
    {
        /// <summary>
        /// 根据请求内容返回相应的请求处理类
        /// </summary>
        /// <param name="content">请求字符串</param>
        /// <param name="serviceProvider">服务提供者实例</param>
        /// <returns>请求处理类实例</returns>
        public static IBusinessHandler GetHandler(string content,IServiceProvider serviceProvider)
        {
            var flagStr = "";

            #region 支付宝预授权
            //flagStr = "AlipayAuthCancel|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayAuthCancelHandler(contentWithoutFlag, log);
            //}
            //flagStr = "AlipayAuthFinish|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayAuthFinishHandler(contentWithoutFlag, log);
            //}
            //flagStr = "AlipayAuthFreeze|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayAuthFreezeHandler(contentWithoutFlag, log);
            //}
            //flagStr = "AlipayAuthQuery|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayAuthQueryHandler(contentWithoutFlag, log);
            //}
            //flagStr = "AlipayAuthUnfreeze|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayAuthUnFreezeHandler(contentWithoutFlag, log);
            //}
            //flagStr = "AlipayAuthQrcode|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayAuthQrcodeHandler(contentWithoutFlag, log);
            //}
            #endregion

            #region 支付宝支付
            //flagStr = "AlipayBarcodePay|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayBarcodePayHandler(contentWithoutFlag, log);
            //}
            //flagStr = "AlipayCloseOrder|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayCloseOrderHandler(contentWithoutFlag, log);
            //}
            //flagStr = "AlipayQrcodePay|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayQrcodePayHandler(contentWithoutFlag, log);
            //}
            //flagStr = "AlipayQuery|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayPayQueryHandler(contentWithoutFlag, log);
            //}
            //flagStr = "AlipayRefund|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayRefundHandler(contentWithoutFlag, log);
            //}
            //flagStr = "AlipayRefundQuery|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new AlipayRefundQueryHandler(contentWithoutFlag, log);
            //}
            #endregion
            #region 微信服务商押金支付
            flagStr = "WxProviderDepositMicropay|";
            if (content.StartsWith(flagStr)) {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderDepositMicropayHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "WxProviderDepositOrderQuery|";
            if (content.StartsWith(flagStr)) {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderDepositOrderQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "WxProviderDepositReverse|";
            if (content.StartsWith(flagStr)) {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderDepositReverseHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "WxProviderDepositConsume|";
            if (content.StartsWith(flagStr)) {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderDepositConsumeHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "WxProviderDepositRefund|";
            if (content.StartsWith(flagStr)) {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderDepositRefundHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "WxProviderDepositRefundQuery|";
            if (content.StartsWith(flagStr)) {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderDepositRefundQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            #endregion

            #region 微信服务商支付
            //flagStr = "WxProviderBarcodePay|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new WxProviderBarcodePayHandler(contentWithoutFlag, log);
            //}
            //flagStr = "WxProviderCloseOrder|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new WxProviderCloseOrderHandler(contentWithoutFlag, log);
            //}
            flagStr = "WxProviderQrcodePay|";
            if (content.StartsWith(flagStr)) {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderQrcodePayHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            //flagStr = "WxProviderQuery|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new WxProviderQueryHandler(contentWithoutFlag, log);
            //}
            //flagStr = "WxProviderRefund|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    if (string.IsNullOrWhiteSpace(javaUrl))
            //    {
            //        return new WxProviderRefundHandler(contentWithoutFlag, log);
            //    } else
            //    {
            //        return new WxProviderRefundViaJavaHandler(contentWithoutFlag, log, javaUrl);
            //    }
            //}
            //flagStr = "WxProviderRefundQuery|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new WxProviderRefundQueryHandler(contentWithoutFlag, log);
            //}
            #endregion

            #region 票付通
            //flagStr = "QueryOrderViaPFT|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new PFTQueryOrderUdpContentHandler(contentWithoutFlag, log);
            //}
            //flagStr = "CheckTicketViaPFT|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new PFTCheckTicketUdpContentHandler(contentWithoutFlag, log);
            //}
            #endregion

            #region 智游宝
            //flagStr = "QueryOrderViaZYB|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new ZYBOrderQueryUdpContentHandler(contentWithoutFlag, log);
            //}
            //flagStr = "CheckTicketViaZYB|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new ZYBOrderCheckUdpContentHandler(contentWithoutFlag, log);
            //}
            #endregion

            #region 广电运通
            //flagStr = "QueryOrderViaGRG|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new GRGBookingOrderQueryUdpContentHandler(contentWithoutFlag, log);
            //}
            //flagStr = "CheckTicketViaGRG|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new GRGBookingOrderCheckUdpContentHandler(contentWithoutFlag, log);
            //}
            #endregion

            #region 自我游
            //flagStr = "QueryOrderViaZWY|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new ZWYOrderQueryUdpContentHandler(contentWithoutFlag, log);
            //}
            //flagStr = "CheckTicketViaZWY|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new ZWYOrderCheckUdpContentHandler(contentWithoutFlag, log);
            //} 
            #endregion
            #region 利楚商务扫呗支付
            flagStr = "LcswPayPrepay|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayPrepayHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            } 
            #endregion
            //http请求转发
            //flagStr = "HttpSwitch|";
            //if (content.StartsWith(flagStr)) {
            //    var contentWithoutFlag = content.Substring(flagStr.Length);
            //    return new HttpSwitchHandler(contentWithoutFlag, log);
            //} 

            //没有任何特殊标记的，则认为是易宝支付，直接返回易宝支付的处理类
            //return new YeePayUdpContentHandler(content);
            throw new ApplicationException("不支持的业务字符串");
        }
    }
}
