using System;
using System.Threading.Tasks;
using GemstarPaymentCore.Payment.ChinaumsPay;
using GemstarPaymentCore.Payment.ChinaumsPay.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GemstarPaymentCore.Business.BusinessHandlers.ChinaumsPay
{
    /// <summary>
    /// 银商支付二维码支付处理类
    /// </summary>
    public class ChinaumsPayQrcodePayHandler : BusinessHandlerBase
    {
        private ILogger _log;
        private readonly IChinaumsPayClient _client;
        private readonly ChinaumsPayOption _options;
        public ChinaumsPayQrcodePayHandler(ILogger<ChinaumsPayQrcodePayHandler> log, IChinaumsPayClient client, IOptionsSnapshot<ChinaumsPayOption> options)
        {
            _log = log;
            _client = client;
            _options = options.Value;
        }
        protected override string contentFormat => "mid|tid|signKey|msgSrc|billNo|billDate|billDesc|totalAmount";
        protected override int[] contentEncryptedIndexs => new int[] { 0 };

        protected override async Task<HandleResult> DoHandleBusinessContentAsync(string[] infos)
        {
            try
            {
                int i = 0;
                var mid = infos[i++];
                var tid = infos[i++];
                var signKey = infos[i++];
                var msgSrc = infos[i++];
                var billNo = infos[i++];
                var billDate = infos[i++];
                var billDesc = infos[i++];
                var totalAmount = infos[i++];
                _options.MId = mid;
                _options.TId = tid;
                _options.SignKey = signKey;
                if (string.IsNullOrEmpty(_options.MId))
                {
                    return HandleResult.Fail("请指定银商商户号信息");
                }
                if (string.IsNullOrEmpty(_options.TId) || string.IsNullOrEmpty(_options.SignKey))
                {
                    return HandleResult.Fail("请指定银商终端号或签名密钥");
                }

                var request = new ChinaumsPayQrcodeGetRequest
                {
                    BillNo = billNo,
                    BillDate = billDate,
                    BillDesc = billDesc,
                    MsgSrc = msgSrc,
                    TotalAmount = Convert.ToInt32(Convert.ToDecimal(totalAmount) * 100)
                };
                var response = await _client.ExecuteRequestAsync(request, _options);

                if (response.IsSuccessCode())
                {
                    return HandleResult.Success($"{response.QrCodeId}|{response.BillQRCode}");
                }
                return response.FailResult();
            } catch (Exception ex)
            {
                return HandleResult.Fail(ex);
            }
        }
    }
}
