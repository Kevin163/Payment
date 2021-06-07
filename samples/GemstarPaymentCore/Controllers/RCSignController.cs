﻿using GemstarPaymentCore.Business;
using GemstarPaymentCore.Models;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace GemstarPaymentCore.Controllers
{
    /// <summary>
    /// 用于处理与rc单相关的所有交互
    /// rc单业务流程和相关说明，请参考https://www.yuque.com/gemstar/za75g6/chkvea
    /// </summary>
    public class RCSignController : Controller
    {
        /// <summary>
        /// bill type of rc
        /// </summary>
        private const string BillTypeRC = "RC单";
        private RCSignHub _rcSignHub;
        private BusinessOption _businessOption;
        private float _right, _bottom;
        private IServiceProvider _serviceProvider;
        public RCSignController(RCSignHub rcSignHub, IOptions<BusinessOption> businessOption,IServiceProvider serviceProvider)
        {
            _rcSignHub = rcSignHub;
            _businessOption = businessOption.Value;
            _serviceProvider = serviceProvider;
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
        /// <param name="amount">the amount should pay</param>
        /// <param name="inputPhoneNo">whether need the user to input phone no,when it's value equal to 1,then user must input phone no,else the user will see the payment qrcode when sign successed</param>
        /// <param name="phoneNo">the default phone no</param>
        /// <param name="alipayQrcode">the payment qrcode content for alipay</param>
        /// <param name="billType">bill type</param>
        /// <returns>notify result</returns>
        public async Task<IActionResult> ShowRC(IFormFile pdf, string regid, string qrcode, string deviceId, string amount,int? inputPhoneNo,string phoneNo,string alipayQrcode,string billType)
        {
            try
            {
                if (string.IsNullOrEmpty(billType))
                {
                    billType = BillTypeRC;
                }
                if (pdf == null || pdf.Length <= 0)
                {
                    return Json(JsonResultData.Failure("pdf file can not be null or empty"));
                }
                if (string.IsNullOrEmpty(regid))
                {
                    return Json(JsonResultData.Failure("regid can not be null or empty"));
                }
                if (string.IsNullOrEmpty(qrcode))
                {
                    if (billType == BillTypeRC)
                    {
                        return Json(JsonResultData.Failure("qrcode can not be null or empty"));
                    }
                    qrcode = "";
                }
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "rcpdfs");
                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
                var fileName = $"{regid}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.pdf";
                using (var stream = System.IO.File.Create(Path.Combine(path, fileName)))
                {
                    await pdf.CopyToAsync(stream);
                    stream.Close();
                }

                var imageName = ConvertPdf2Png(path, fileName);
                var pdfUri = Url.Content($"rcpdfs/{fileName}");
                var imageUri = Url.Content($"rcpdfs/{imageName}");
                var rcInfo = new { regid, pdfFileName=fileName, qrcode, amount, imageUrl=imageUri, needInputPhoneNo = (inputPhoneNo ?? 1).ToString(), defaultPhoneNo= phoneNo ?? "", alipayQrcode= alipayQrcode ?? "",billType };
                await _rcSignHub.ShowRC(deviceId, JsonConvert.SerializeObject(rcInfo));
                return Json(JsonResultData.Successed("notify android device to show rc pdf successed"));
            }
            catch(Exception ex)
            {
                return Json(JsonResultData.Failure(ex));
            }
        }
        private string ConvertPdf2Png(string path,string pdfFileName)
        {
            var imageNamePrefix = Path.GetFileNameWithoutExtension(pdfFileName);
            var processInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PdfRenderer", "pdftopng.exe"),
                Arguments = $"-f 1 -l 1 {Path.Combine(path,pdfFileName)} {Path.Combine(path,imageNamePrefix)}"
            };
            var process = Process.Start(processInfo);
            process.WaitForExit();
            var imageFileNames = Directory.GetFiles(path,$"{imageNamePrefix}*.png");
            if(imageFileNames == null || imageFileNames.Length == 0)
            {
                throw new ApplicationException("pdf转换为图片失败，请检查wwwroot目录下的PdfRenderer目录是否存在");
            }
            return Path.GetFileName(imageFileNames[0]);

        }
        /// <summary>
        /// save the signed image to rc pdf
        /// </summary>
        /// <param name="imageBase64Data">image base64 data</param>
        /// <param name="regid">regid for guest checkin</param>
        /// <param name="pdfName">the origial pdf file name</param>
        /// <returns>save status</returns>
        public IActionResult SaveSignImageToPdf(string imageBase64Data, string regid, string pdfName,string billType)
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
                    if (_businessOption.RCSystemVersion == BusinessOption.SystemBsPms)
                    {
                        SaveSignedPdfToBsPms(billType, regid, outputPdfStream.ToArray());
                    }
                    else
                    {
                        SaveSignedPdfToDB(billType, regid, outputPdfStream.ToArray());
                    }
                    return Json(JsonResultData.Successed());
                }
            }
            catch (Exception ex)
            {
                return Json(JsonResultData.Failure(ex));
            }
        }

        private void SaveSignedPdfToBsPms(string billType, string regid, byte[] pdfBytes)
        {
            if (string.IsNullOrEmpty(billType))
            {
                billType = BillTypeRC;
            }
            if (string.IsNullOrEmpty(_businessOption.RCSignBsPmsApiUrl))
            {
                throw new ApplicationException("请先配置bspms对接电子签名的接口地址");
            }
            var apiUrl = $"{_businessOption.RCSignBsPmsApiUrl}/SaveSignedPdf";
            var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>> { 
            new KeyValuePair<string, string>("billType",billType),
            new KeyValuePair<string, string>("billNo",regid),
            new KeyValuePair<string, string>("pdfSignData",Convert.ToBase64String(pdfBytes))
            });
            var response = httpClient.PostAsync(apiUrl, formContent).Result;
            if (response.IsSuccessStatusCode)
            {
                var resultStr = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<JsonResultData>(resultStr);
                if (!result.Success)
                {
                    throw new ApplicationException($"调用bspms 接口{apiUrl}保存签名pdf时失败，原因：{result.Data}");
                }
            }
            throw new ApplicationException($"调用bspms 接口{apiUrl}保存签名pdf时失败，状态码：{response.StatusCode}");
        }

        /// <summary>
        /// save the signed image with origial pdf to db
        /// </summary>
        /// <param name="regid">regid</param>
        /// <param name="pdfBytes"></param>
        private void SaveSignedPdfToDB(string billType,string regid, byte[] pdfBytes)
        {
            if (string.IsNullOrEmpty(billType))
            {
                billType = BillTypeRC;
            }
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

                var pBillType = cmd.CreateParameter();
                pBillType.ParameterName = "pdfType";
                pBillType.Value = billType;
                cmd.Parameters.Add(pBillType);

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
        public IActionResult SavePhoneNo(string regid, string phoneNo)
        {
            if (string.IsNullOrEmpty(regid))
            {
                return Json(JsonResultData.Failure("regid must have a valid value"));
            }
            if (string.IsNullOrEmpty(phoneNo))
            {
                return Json(JsonResultData.Failure("phone no must have a valid value"));
            }
            try
            {
                if (_businessOption.RCSystemVersion == BusinessOption.SystemBsPms)
                {
                    SavePhoneNoToBsPms(regid,phoneNo);
                }
                else
                {
                    SavePhoneNoToDb(regid, phoneNo);
                }
                return Json(JsonResultData.Successed());
            }
            catch (Exception ex)
            {
                return Json(JsonResultData.Failure(ex));
            }
        }

        private void SavePhoneNoToBsPms(string regid, string phoneNo)
        {
            if (string.IsNullOrEmpty(phoneNo))
            {
                throw new ApplicationException("请先输入手机号");
            }
            if (string.IsNullOrEmpty(_businessOption.RCSignBsPmsApiUrl))
            {
                throw new ApplicationException("请先配置bspms对接电子签名的接口地址");
            }
            var apiUrl = $"{_businessOption.RCSignBsPmsApiUrl}/SavePhoneNo";
            var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("billNo",regid),
            new KeyValuePair<string, string>("phoneNo",phoneNo)
            });
            var response = httpClient.PostAsync(apiUrl, formContent).Result;
            if (response.IsSuccessStatusCode)
            {
                var resultStr = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<JsonResultData>(resultStr);
                if (!result.Success)
                {
                    throw new ApplicationException($"调用bspms 接口{apiUrl}保存客人手机时失败，原因：{result.Data}");
                }
            }
            throw new ApplicationException($"调用bspms 接口{apiUrl}保存客人手机时失败，状态码：{response.StatusCode}");
        }

        private void SavePhoneNoToDb(string regid, string phoneNo)
        {
            var connStr = _businessOption.Systems.Where(w => w.Name == "KF").FirstOrDefault()?.ConnStr;
            if (string.IsNullOrEmpty(connStr))
            {
                throw new ApplicationException("使用rc电子签名功能时，必须正确配置客房数据库连接信息，请检查配置文件");
            }
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "up_gsSaveRCSignPhoneNo_Regid";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var pRegId = cmd.CreateParameter();
                pRegId.ParameterName = "regid";
                pRegId.Value = regid;
                cmd.Parameters.Add(pRegId);

                var pCreator = cmd.CreateParameter();
                pCreator.ParameterName = "creator";
                pCreator.Value = "rcSign";
                cmd.Parameters.Add(pCreator);

                var pPhoneNo = cmd.CreateParameter();
                pPhoneNo.ParameterName = "PhoneNo";
                pPhoneNo.Value = phoneNo;
                cmd.Parameters.Add(pPhoneNo);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// notify the device to show the payment qrcode again
        /// because the use may change the page to home or other pgae,but did not payment success
        /// </summary>
        /// <param name="regid">regid for guest checkin</param>
        /// <param name="deviceId">the android id from ini config file</param>
        /// <returns></returns>
        public async Task<IActionResult> RePay(string regid, string deviceId)
        {
            try
            {
                await _rcSignHub.RePay(deviceId, regid);
                return Json(JsonResultData.Successed());
            }catch(Exception ex)
            {
                return Json(JsonResultData.Failure(ex));
            }
        }
        /// <summary>
        /// notify the device the go back to home,because use payment successed
        /// </summary>
        /// <param name="regid">regid for guest checkin</param>
        /// <param name="deviceId">the android device id from ini config file</param>
        /// <param name="successShowSeconds">the seconds of show payment result</param>
        public async Task<IActionResult> GoBackHome(string regid,string deviceId,int? successShowSeconds)
        {
            int seconds = successShowSeconds ?? 3;
            await _rcSignHub.ShowHome(deviceId, regid,seconds);
            return Json(JsonResultData.Successed());
        }
    }
}
