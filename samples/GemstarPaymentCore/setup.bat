@echo on
@echo ��ʼ׼����װ���Ŵ�֧���м������d:\jxdsystem\paymentCore��Ҫ������Ѿ���װ��git
@if not exist d:\jxdsystem @mkdir d:\jxdsystem
@if not exist d:\jxdsystem\paymentCore (
	@echo ��ʼ���س����Ҵ���Ŀ¼
	d:
	cd d:\jxdsystem
	git clone https://gitee.com/chenql365/publishCorePayment.git paymentCore
	if errorlevel 1 (
		@echo ����git�����Ƿ�װ���밲װ��ɺ��������б�����
	) else (
		cd paymentCore
		git config user.name "jxdsystem"
		git config user.email "jxdsystem@gshis.com"
		@echo �����Ѿ���װ��ɣ����޸�d:\jxdsystem\paymentCore\appsettings.json�ļ��е�������Ϣ
		@echo ������������Ϣ��������saveconfig.bat�ļ��������޸�
	)
) else (
	d:
	cd d:\jxdsystem\paymentCore
	git checkout .
	git pull
	if errorlevel 1 (
		@echo �Ѿ����ڵ�Ŀ¼����ʹ�ô˹�������ʼ��װ�ģ���������ƻ���ɾ�����������б�����
	) else (
		@echo ��ȡ���³������
		@start startAndHide.vbs
	)
)
