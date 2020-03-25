using System;
using GemstarPaymentCore.Business.Utility;
using GemstarPaymentCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace GemstarPaymentCore.Controllers
{
    public class GetSignController : Controller
    {
        private ISecurity _security;
        public GetSignController(ISecurity security)
        {
            _security = security;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CalcSign(SignViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(JsonResultData.Failure("请输入要加密的内容和密钥"));
                }
                var encrypted = _security.Encrypt(model.Content, model.Key);
                return Json(JsonResultData.Successed(encrypted));

            } catch (Exception ex)
            {
                return Json(JsonResultData.Failure(ex));
            }
        }
    }
}