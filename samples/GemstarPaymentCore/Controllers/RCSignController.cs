using GemstarPaymentCore.Business;
using GemstarPaymentCore.Models;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Controllers
{
    /// <summary>
    /// 用于处理与rc单相关的所有交互
    /// rc单业务流程和相关说明，请参考https://www.yuque.com/gemstar/za75g6/chkvea
    /// </summary>
    public class RCSignController : Controller
    {
        private RCSignHub _rcSignHub;
        private BusinessOption _businessOption;
        private float _right, _bottom;
        public RCSignController(RCSignHub rcSignHub, IOptions<BusinessOption> businessOption)
        {
            _rcSignHub = rcSignHub;
            _businessOption = businessOption.Value;
            if (float.TryParse(_businessOption.RCSignPositionRight, out float rightValue))
            {
                _right = rightValue;
            }
            else
            {
                _right = 100f;
            }
            if (float.TryParse(_businessOption.RCSignPositionBottom, out float bottomValue))
            {
                _bottom = bottomValue;
            }
            else
            {
                _bottom = 100f;
            }
        }
        /// <summary>
        /// rc单签名设备端显示的默认首页
        /// 目前支持直接显示一些图片轮播，图片可以由用户上传后保存到目录~/ADImages下面
        /// 这里将直接取出后给前端进行显示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// receive the show rc pdf in android notify
        /// cs pms will call this action when rc pdf ready
        /// </summary>
        /// <param name="pdf">rc pdf file content</param>
        /// <param name="regid">the id for guest checkin</param>
        /// <param name="qrcode">the payment qrcode content</param>
        /// <param name="deviceId">the android device id associated to the cs pms workstation</param>
        /// <returns>notify result</returns>
        public async Task<IActionResult> ShowRC(IFormFile pdf, string regid, string qrcode, string deviceId, string amount)
        {
            try
            {
                if (pdf == null || pdf.Length <= 0)
                {
                    return Json(JsonResultData.Failure("pdf file cat not be null or empty"));
                }
                if (string.IsNullOrEmpty(regid))
                {
                    return Json(JsonResultData.Failure("regid cat not be null or empty"));
                }
                if (string.IsNullOrEmpty(qrcode))
                {
                    return Json(JsonResultData.Failure("qrcode can not be null or empty"));
                }
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "rcpdfs");
                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
                var fileName = $"{regid}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.pdf";
                using (var stream = System.IO.File.Create(Path.Combine(path, fileName)))
                {
                    await pdf.CopyToAsync(stream);
                    stream.Close();
                }
                var pdfUri = Url.Content($"rcpdfs/{fileName}");
                await _rcSignHub.ShowRC(deviceId, pdfUri, qrcode, regid, amount);
                return Json(JsonResultData.Successed("notify android device to show rc pdf successed"));
            }
            catch(Exception ex)
            {
                return Json(JsonResultData.Failure(ex));
            }
        }
        /// <summary>
        /// save the signed image to rc pdf
        /// </summary>
        /// <param name="imageBase64Data">image base64 data</param>
        /// <param name="regid">regid for guest checkin</param>
        /// <param name="pdfName">the origial pdf file name</param>
        /// <returns>save status</returns>
        public IActionResult SaveSignImageToPdf(string imageBase64Data, string regid, string pdfName)
        {
            try
            {
                var oldPdfName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "rcpdfs", Path.GetFileName(pdfName));
                using (var inputPdfStream = System.IO.File.Open(oldPdfName, FileMode.Open))
                using (var outputPdfStream = new MemoryStream())
                {
                    var pdfDocument = new PdfDocument(new PdfReader(inputPdfStream), new PdfWriter(outputPdfStream));
                    var document = new Document(pdfDocument);
                    var imageData = ImageDataFactory.Create(Convert.FromBase64String(imageBase64Data));
                    var img = new Image(imageData).Scale(0.2f, 0.2f);
                    var pageWidth = pdfDocument.GetPage(1).GetPageSizeWithRotation().GetWidth();
                    var imageWidth = img.GetImageScaledWidth();
                    var left = pageWidth - imageWidth - _right;
                    img.SetFixedPosition(left, _bottom);
                    document.Add(img);
                    document.Close();
                    SaveSignedPdfToDB(regid, outputPdfStream.ToArray());
                    return Json(JsonResultData.Successed());
                }
            }
            catch (Exception ex)
            {
                return Json(JsonResultData.Failure(ex));
            }
        }
        /// <summary>
        /// save the signed image with origial pdf to db
        /// </summary>
        /// <param name="regid">regid</param>
        /// <param name="pdfBytes"></param>
        private void SaveSignedPdfToDB(string regid, byte[] pdfBytes)
        {
            var connStr = _businessOption.Systems.Where(w => w.Name == "KF").FirstOrDefault()?.ConnStr;
            if (string.IsNullOrEmpty(connStr))
            {
                throw new ApplicationException("使用rc电子签名功能时，必须正确配置客房数据库连接信息，请检查配置文件");
            }
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "up_gsSaveRCSignPdf_Regid";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var pRegId = cmd.CreateParameter();
                pRegId.ParameterName = "regid";
                pRegId.Value = regid;
                cmd.Parameters.Add(pRegId);

                var pCreator = cmd.CreateParameter();
                pCreator.ParameterName = "creator";
                pCreator.Value = "rcSign";
                cmd.Parameters.Add(pCreator);

                var pPdf = cmd.CreateParameter();
                pPdf.ParameterName = "pdfSignData";
                pPdf.Value = pdfBytes;
                cmd.Parameters.Add(pPdf);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// notify the device the go back to home,because use payment successed
        /// </summary>
        /// <param name="regid">regid for guest checkin</param>
        /// <param name="deviceId">the android device id from ini config file</param>
        public async Task<IActionResult> GoBackHome(string regid,string deviceId)
        {
            await _rcSignHub.ShowHome(deviceId, regid);
            return Json(JsonResultData.Successed());
        }
    }
}
