using System;
using System.Collections.Generic;
using System.Text;
using GemstarPaymentCore.Payment.ChinaumsPay.Utility;
using Xunit;

namespace GemstarPaymentCore.Test
{
    public class ChinaumsPaySignatureTest
    {
        [Fact]
        public void SignWithKeyShouldEqualToExampleTest()
        {
            var content = new SortedDictionary<string, string> {
                { "walletOption","SINGLE"},
                { "billNo","31940000201700002"},
                { "billDate","2017-06-26"},
                { "sign","2631915B7F7822C4B00A488A32E03764"},
                { "requestTimestamp","2017-06-26 17:28:02"},
                { "instMid","QRPAYDEFAULT"},
                { "msgSrc","WWW.TEST.COM"},
                { "totalAmount","1"},
                { "goods","[{\"body\":\"微信二维码测试\",\"price\":\"1\",\"goodsName\":\"微信二维码测试\",\"goodsId\":\"1\",\"quantity\":\"1\",\"goodsCategory\":\"TEST\"}]"},
                { "msgType","bills.getQRCode"},
                { "mid","898340149000005"},
                { "tid","88880001"},
                { "signType","SHA256"}
            };
            var key = "fcAmtnx7MwismjWNhNKdHC44mNXtnEQeJkRrhKJwyrW2ysRR";
            var expectedSign = "a9eced8dd8425d1fc4047cf94e672c69ed1073557ee831c51287341cfab0b21f";
            var actualSign = ChinaumsPaySignature.SignWithKey(content, key);
            Assert.Equal(expectedSign, actualSign);
        }
    }
}
