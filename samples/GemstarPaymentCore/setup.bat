@echo on
@echo 开始准备安装捷信达支付中间件程序到d:\jxdsystem\paymentCore，要求必须已经安装好git
@if not exist d:\jxdsystem @mkdir d:\jxdsystem
@if not exist d:\jxdsystem\paymentCore (
	@echo 开始下载程序并且创建目录
	d:
	cd d:\jxdsystem
	git clone https://gitee.com/chenql365/publishCorePayment.git paymentCore
	if errorlevel 1 (
		@echo 请检查git环境是否安装，请安装完成后重新运行本程序
	) else (
		cd paymentCore
		git config user.name "jxdsystem"
		git config user.email "jxdsystem@gshis.com"
		@echo 程序已经安装完成，请修改d:\jxdsystem\paymentCore\appsettings.json文件中的配置信息
		@echo 更改完配置信息后，请运行saveconfig.bat文件来保存修改
	)
) else (
	d:
	cd d:\jxdsystem\paymentCore
	git checkout .
	git pull
	if errorlevel 1 (
		@echo 已经存在的目录不是使用此工具来初始安装的，请更改名称或者删除后重新运行本程序
	) else (
		@echo 获取最新程序完成
		@start startAndHide.vbs
	)
)
