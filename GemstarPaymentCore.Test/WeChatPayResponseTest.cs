using System;
using Essensoft.AspNetCore.Payment.WeChatPay.Parser;
using Essensoft.AspNetCore.Payment.WeChatPay.Response;
using Essensoft.AspNetCore.Payment.WeChatPay.Utility;
using GemstarPaymentCore.Business;
using Xunit;

namespace GemstarPaymentCore.Test
{
    public class WeChatPayResponseTest
    {
        [Fact]
        public void ResponseXmlParserShouldParseOKTest()
        {
            var responseXml = @"<xml><return_code><![CDATA[SUCCESS]]></return_code>
<return_msg><![CDATA[OK]]></return_msg>
<appid><![CDATA[wx4cea1ae3f21c72e8]]></appid>
<mch_id><![CDATA[1345752201]]></mch_id>
<sub_mch_id><![CDATA[1500560691]]></sub_mch_id>
<nonce_str><![CDATA[sX2I8bNkeUQRbUyk]]></nonce_str>
<sign><![CDATA[49FCAAAE64CB8EDCAD2CD91FB5638D77]]></sign>
<result_code><![CDATA[SUCCESS]]></result_code>
<openid><![CDATA[o0_O0t1U0OQXtgm5w9kafhdQFUdA]]></openid>
<is_subscribe><![CDATA[N]]></is_subscribe>
<trade_type><![CDATA[MICROPAY]]></trade_type>
<bank_type><![CDATA[GDRCU_DEBIT]]></bank_type>
<total_fee>10</total_fee>
<fee_type><![CDATA[CNY]]></fee_type>
<transaction_id><![CDATA[4200000443201912243367650720]]></transaction_id>
<out_trade_no><![CDATA[013I2L1Q1Z271C112E8S330Y9A241812]]></out_trade_no>
<attach><![CDATA[]]></attach>
<time_end><![CDATA[20191224111837]]></time_end>
<sub_appid><![CDATA[wx6d1d2d3ece99ed30]]></sub_appid>
<sub_openid><![CDATA[oG9dzxAshKr4R8gGDRXLuAzVs1WY]]></sub_openid>
<sub_is_subscribe><![CDATA[Y]]></sub_is_subscribe>
<cash_fee>10</cash_fee>
<cash_fee_type><![CDATA[CNY]]></cash_fee_type>
</xml>";
            var responseParser = new WeChatPayXmlParser<WeChatPayMicroPayResponse>();
            var response = responseParser.Parse(responseXml);

            Assert.NotNull(response);
            Assert.True(response.Parameters.Count > 0);
            var sign = response.Parameters.GetValue("sign");
            var cal_sign = WeChatPaySignature.SignWithKey(response.Parameters, ConfigHelper.WxProviderKey, true);
            Assert.Equal(sign, cal_sign);
        }
    }
}
