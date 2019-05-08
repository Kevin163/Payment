﻿drop table unionPay_lcsw
create table unionPay_lcsw(
		Id uniqueidentifier not null,--主键值
		terminalTrace varchar(60) not null,--终端流水号，填写商户系统的订单号
		terminalTime datetime not null,--终端交易时间，yyyyMMddHHmmss
		merchantNo varchar(60) not null,--扫呗商户号
		terminalId varchar(60) not null,--扫呗终端号
		accessToken VARCHAR(60) not null,--终端访问令牌
		outletCode varchar(30) null,--营业点代码
		totalFee decimal(18,2) not null,--金额，单位元
		orderBody varchar(200) not null,--订单描述
		attach varchar(200) null,--附加数据，原样返回
		callbackUrl varchar(2000) null,--支付回调地址
		memberUrl varchar(2000) null,--会员接口地址
		memberType varchar(30) null,--会员类型
		lcswPayUnionQrcodeUrl varchar(2000) null,--扫呗支付聚合支付的二维码链接地址
		appId varchar(32) null,--微信公众号id
		appSecret varchar(64) null,--微信公众号密钥
		status int not null,--状态
		paytime datetime null,--支付时间
		payTransId varchar(64) null,--支付流水号，用于对账
		payRemark varchar(200) null,--支付备注
		CONSTRAINT pk_unionPay_lcsw PRIMARY key(id)
)