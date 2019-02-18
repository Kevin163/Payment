﻿using System;
using System.Threading.Tasks;
using GemstarPaymentCore.Business;
using GemstarPaymentCore.Business.BusinessHandlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GemstarPaymentCore.Controllers
{
    public class ClientPayController : Controller
    {
        private ILogger<ClientPayController> _logger;
        private IServiceProvider _serviceProvider;
        private EventId _eventId = new EventId(1,nameof(ClientPayController));
        public ClientPayController(ILogger<ClientPayController> logger,IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public async Task<IActionResult> Index(string payStr)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(payStr))
                {
                    _logger.LogInformation(_eventId,$"接收到的业务字符串：{payStr}");

                    var handler = BusinessHandlerFactory.GetHandler(payStr,_serviceProvider);
                    var result = await handler.HandleBusinessContentAsync();
                    _logger.LogInformation(_eventId,$"返回的业务字符串：{result.ResultStr}");
                    return Content(result.ResultStr);
                } else
                {
                    var result = HandleResult.Fail("请指定payStr参数内容");
                    return Content(result.ResultStr);
                }
            } catch (Exception ex)
            {
                _logger.LogError(_eventId, ex, "执行业务时遇到异常");
                var result = HandleResult.Fail(ex);
                return Content(result.ResultStr);
            }
        }
    }
}