@echo on
@echo �˳��򽫱����Ѿ��޸ĵ�������Ϣ���Է�ֹ�����ȡ����ʱ��ʧû�б���ĸ���
d:
cd d:\jxdsystem\paymentCore
@echo ��ʼ����������Ϣ
git add appsettings.json
git commit -m "save the modified appsettings.json"
@if errorlevel 1 (
	@echo ����ʧ�ܣ��뽫��ǰ�����ͼ������Ŵ���ϵ
) else (
	@echo ����ɹ�
)