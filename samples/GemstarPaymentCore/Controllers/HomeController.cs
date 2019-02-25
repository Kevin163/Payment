using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GemstarPaymentCore.Models;
using System;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.LcswPay.Request;
using Essensoft.AspNetCore.Payment.LcswPay;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace GemstarPaymentCore.Controllers
{
    public class HomeController : Controller
    {
        private IServiceProvider _serviceProvider;
        public HomeController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult LcswPaySign()
        {
            var viewModel = new LcswPaySignViewModel
            {
                TerminalTime = DateTime.Now.ToString("yyyyMMddHHmmss"),
                TerminalTrace = $"trace{DateTime.Now.ToString("yyyyMMddHHmmss")}"
            };
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> LcswPaySign(LcswPaySignViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var request = new LcswPaySignRequest
                {
                    ServiceId = "090",
                    MerchantNo = viewModel.MerchantNo,
                    TerminalId = viewModel.TerminalId,
                    TerminalTime = viewModel.TerminalTime,
                    TerminalTrace = viewModel.TerminalTrace
                };
                var client = _serviceProvider.GetService<ILcswPayClient>();
                var response = await client.ExecuteAsync(request);
                ViewData["response"] = response.Body;
                ViewData["token"] = response.AccessToken;
            }
            return View(viewModel);
        }
    }
}
