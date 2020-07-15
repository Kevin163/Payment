--增加利楚商务扫呗聚合支付业务表
if object_id('unionPay_lcsw') is null
begin
	create table unionPay_lcsw(
			Id uniqueidentifier not null,--主键值
			systemName varchar(20) not null,--用于记录是哪个业务系统的单据，以便回调通知时知道修改哪个业务系统中的数据
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
			payType varchar(10) null,--支付方式，Member:会员卡支付,010：微信，020：支付宝，060：qq钱包，080：京东钱包，090：口碑，100：翼支付
			CONSTRAINT pk_unionPay_lcsw PRIMARY key(id)
	)
end
--给利楚商务扫呗聚合支付业务表增加会员绑定地址列，用于当检测到还不是会员时进行会员绑定
if not exists(select * from syscolumns where id=object_id('unionPay_lcsw') and name = 'memberBindUrl')
BEGIN
	ALTER TABLE unionPay_lcsw add memberBindUrl varchar(2000) null
END
--给利楚商务扫呗聚合支付业务表增加会员参数列，用于会员调用接口时需要传递的额外参数
if not exists(select * from syscolumns where id=object_id('unionPay_lcsw') and name = 'memberPara')
BEGIN
	ALTER TABLE unionPay_lcsw add memberPara varchar(2000) null
END
--增加一个历史表
CREATE TABLE [dbo].[unionPay_lcsw_history] (
  [Id] uniqueidentifier  NOT NULL,
  [systemName] varchar(20) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [terminalTrace] varchar(60) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [terminalTime] datetime  NOT NULL,
  [merchantNo] varchar(60) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [terminalId] varchar(60) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [accessToken] varchar(60) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [outletCode] varchar(30) COLLATE Chinese_PRC_CI_AS  NULL,
  [totalFee] decimal(18,2)  NOT NULL,
  [orderBody] varchar(200) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [attach] varchar(200) COLLATE Chinese_PRC_CI_AS  NULL,
  [callbackUrl] varchar(2000) COLLATE Chinese_PRC_CI_AS  NULL,
  [memberUrl] varchar(2000) COLLATE Chinese_PRC_CI_AS  NULL,
  [memberType] varchar(30) COLLATE Chinese_PRC_CI_AS  NULL,
  [lcswPayUnionQrcodeUrl] varchar(2000) COLLATE Chinese_PRC_CI_AS  NULL,
  [appId] varchar(32) COLLATE Chinese_PRC_CI_AS  NULL,
  [appSecret] varchar(64) COLLATE Chinese_PRC_CI_AS  NULL,
  [status] int  NOT NULL,
  [paytime] datetime  NULL,
  [payTransId] varchar(64) COLLATE Chinese_PRC_CI_AS  NULL,
  [payRemark] varchar(200) COLLATE Chinese_PRC_CI_AS  NULL,
  [payType] varchar(10) COLLATE Chinese_PRC_CI_AS  NULL,
  [memberBindUrl] varchar(2000) COLLATE Chinese_PRC_CI_AS  NULL,
  [memberPara] varchar(2000) COLLATE Chinese_PRC_CI_AS  NULL,
  CONSTRAINT [pk_unionPay_lcsw_copy1] PRIMARY KEY CLUSTERED ([Id])
)  
--增加聚合支付的支付明细表，用于记录支付时使用会员优惠券信息和剩余金额的支付信息
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'unionPay_lcsw_detail')
begin
	create table unionPay_lcsw_detail(
		detailId uniqueidentifier not null,		--明细id，主键值
		payId uniqueidentifier not null,		--支付记录id
		amount numeric(18,2) not null,			--应付金额
		cdate datetime not null,						--创建时间
		payStatus int not null,				--支付状态，50：待支付，3：已支付，4：支付失败，5：已取消
		payQrcodeUrl varchar(2000) null,		--二维码支付地址
		payType varchar(30) not null,			--支付方式：ticket:会员优惠券支付，lcsw:扫呗聚合支付
		paidAmount numeric(18,2) null,			--实际支付金额
		paidTime datetime null,					--实际支付时间
		paidTransNo varchar(60) null,			--实际支付流水号，当为会员优惠券支付时，此时保存的是优惠券的券号
		constraint pk_unionPay_lcsw_detail primary key(detailid)
	)
	create index ix_union_lcsw_detail_payid on unionpay_lcsw_detail(payId)
end
--更改支付方式字段列的宽度，因为增加了使用优惠券的支付，原来只是保存简单的010，020，Member这样的字符串，更改为在这些字符串后面增加会员券的使用信息，格式：原支付方式~原支付方式对应的金额~支付流水号|Ticket~会员优惠券金额~会员优惠券券号，比如Member~200|Ticket~10~123456
if exists(select * from INFORMATION_SCHEMA.columns where table_name = 'unionPay_lcsw' and column_name = 'payType' and character_maximum_length = 10)
begin
	alter table unionPay_lcsw alter column payType varchar(200) null 
end


--增加线下业务系统待退款记录表，陈前良，2019-10-8 18:14:6
if object_id('waitRefundList') is null
begin
	create table waitRefundList(
		refundId varchar(32) not null,--退款id，主键值
		payType varchar(20) not null,--支付类型，将决定了如何进行退款，支持的类型:weixinpay,alipay,lcswpay
		refundPara varchar(max) not null,--退款参数，不同的支付类型需要不同的参数，以json格式进行传递，具体的参数格式见文档https://www.yuque.com/gemstar/works/payment_refund
		refundStatus varchar(30) not null,--退款状态,notSend:未发送退款请求，sended：已发送退款请求，success：退款成功，fail：退款失败，其他中间状态字符串
		refundFailReason varchar(max) null,--退款失败原因
		createDate datetime not null,--创建日期
		sendDate datetime null,--发送退款请求日期
		constraint pk_waitRefundList primary key(refundId)
	)
end