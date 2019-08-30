﻿using GemstarPaymentCore.Business.BusinessHandlers.Alipay;
using GemstarPaymentCore.Business.BusinessHandlers.Gemstar;
using GemstarPaymentCore.Business.BusinessHandlers.LcswPay;
using GemstarPaymentCore.Business.BusinessHandlers.PayWxProvider;
using GemstarPaymentCore.Business.BusinessHandlers.TicketsGRGBooking;
using GemstarPaymentCore.Business.BusinessHandlers.TicketsPFT;
using GemstarPaymentCore.Business.BusinessHandlers.TicketsZhiWoYou;
using GemstarPaymentCore.Business.BusinessHandlers.TicketsZhiYouBao;
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
        /// <param name="para">处理参数实例</param>
        /// <param name="serviceProvider">服务提供者实例</param>
        /// <returns>请求处理类实例</returns>
        public static IBusinessHandler GetHandler(string content,BusinessHandlerParameter para,IServiceProvider serviceProvider)
        {
            var flagStr = "";

            #region 支付宝预授权
            flagStr = "AlipayAuthCancel|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayAuthCancelHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "AlipayAuthFinish|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayAuthFinishHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "AlipayAuthFreeze|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayAuthFreezeHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "AlipayAuthQuery|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayAuthQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "AlipayAuthUnfreeze|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayAuthUnFreezeHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "AlipayAuthQrcode|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayAuthQrcodeHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            #endregion

            #region 支付宝支付
            flagStr = "AlipayBarcodePay|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayBarcodePayHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "AlipayCloseOrder|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayCloseOrderHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "AlipayQrcodePay|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayQrcodePayHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "AlipayQuery|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayPayQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "AlipayRefund|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayRefundHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "AlipayRefundQuery|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<AlipayRefundQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
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
            flagStr = "WxProviderBarcodePay|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderPayBarcodePayHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "WxProviderCloseOrder|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderPayCloseOrderHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "WxProviderQrcodePay|";
            if (content.StartsWith(flagStr)) {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderPayQrcodePayHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "WxProviderQuery|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderPayQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "WxProviderRefund|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderPayRefundHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
                //if (string.IsNullOrWhiteSpace(javaUrl))
                //{
                //    return new WxProviderRefundHandler(contentWithoutFlag, log);
                //} else
                //{
                //    return new WxProviderRefundViaJavaHandler(contentWithoutFlag, log, javaUrl);
                //}
            }
            flagStr = "WxProviderRefundQuery|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<WxProviderPayRefundQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            #endregion

            #region 票付通
            flagStr = "QueryOrderViaPFT|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<PFTQueryOrderUdpContentHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "CheckTicketViaPFT|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<PFTCheckTicketUdpContentHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            #endregion

            #region 智游宝
            flagStr = "QueryOrderViaZYB|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<ZYBOrderQueryUdpContentHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "CheckTicketViaZYB|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<ZYBOrderCheckUdpContentHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            #endregion

            #region 广电运通
            flagStr = "QueryOrderViaGRG|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<GRGBookingOrderQueryUdpContentHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "CheckTicketViaGRG|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<GRGBookingOrderCheckUdpContentHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            #endregion

            #region 自我游
            flagStr = "QueryOrderViaZWY|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<ZWYOrderQueryUdpContentHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "CheckTicketViaZWY|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<ZWYOrderCheckUdpContentHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
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
            flagStr = "LcswPayBarcodePay|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayBarcodePayHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            } 
            flagStr = "LcswPayQuery|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            } 
            flagStr = "LcswPayRefund|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayRefundHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "LcswPayRefundQuery|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayRefundQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "LcswPayUnionQrcodePay|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayUnionQrcodePayHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "LcswPayJsPay|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayJsPayHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            #endregion
            #region 利楚商务扫呗预授权支付
            flagStr = "LcswPayPreAuthBar|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayPreAuthBarHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "LcswPayPreAuthQrcode|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayPreAuthQrcodeHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "LcswPayPreAuthOrderQuery|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayPreAuthOrderQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "LcswPayPreAuthUnionQrcode|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayPreAuthUnionQrcodeHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "LcswPayPreAuthFinish|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayPreAuthFinishHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "LcswPayPreAuthCancel|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayPreAuthCancelHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            flagStr = "LcswPayPreAuthResultQuery|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<LcswPayPreAuthResultQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }
            #endregion

            #region 捷信达聚合支付
            flagStr = "JxdUnionLcswPay|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<JxdUnionLcswPayHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                handler.SetBusiessHandlerParameter(para);
                return handler;
            }
            flagStr = "JxdUnionLcswQuery|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<JxdUnionLcswPayQueryHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                handler.SetBusiessHandlerParameter(para);
                return handler;
            }
            #endregion
            //http请求转发
            flagStr = "HttpSwitch|";
            if (content.StartsWith(flagStr))
            {
                var contentWithoutFlag = content.Substring(flagStr.Length);
                var handler = serviceProvider.GetService<HttpSwitchHandler>();
                handler.SetBusinessContent(contentWithoutFlag);
                return handler;
            }

            //没有任何特殊标记的，则认为是易宝支付，直接返回易宝支付的处理类
            //return new YeePayUdpContentHandler(content);
            throw new ApplicationException("不支持的业务字符串");
        }
    }
}
