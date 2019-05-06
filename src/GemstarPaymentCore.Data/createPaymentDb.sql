drop table unionPay_lcsw
create table unionPay_lcsw(
		terminalTrace varchar(60) not null,--终端流水号，填写商户系统的订单号
		terminalTime datetime not null,--终端交易时间，yyyyMMddHHmmss
		merchantNo varchar(60) not null,--扫呗商户号
		terminalId varchar(60) not null,--扫呗终端号
		accessToken VARCHAR(60) not null,--终端访问令牌
		totalFee decimal(18,2) not null,--金额，单位元
		orderBody varchar(200) not null,--订单描述
		attach varchar(200) null,--附加数据，原样返回
		callbackUrl varchar(2000) null,--支付回调地址
		memberUrl varchar(2000) null,--会员接口地址
		lcswPayUnionQrcodeUrl varchar(2000) null,--扫呗支付聚合支付的二维码链接地址
		CONSTRAINT pk_unionPay_lcsw PRIMARY key(terminalTrace)
)