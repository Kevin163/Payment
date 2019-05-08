using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GemstarPaymentCore.Controllers
{
    /// <summary>
    /// 扫呗聚合支付成功后的回调
    /// </summary>
    public class LcswPayNotifyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}