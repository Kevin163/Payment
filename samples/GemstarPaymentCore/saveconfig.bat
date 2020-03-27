@echo on
@echo 此程序将保存已经修改的配置信息，以防止后面获取程序时丢失没有保存的更改
d:
cd d:\jxdsystem\paymentCore
@echo 开始保存配置信息
git add appsettings.json
git commit -m "save the modified appsettings.json"
@if errorlevel 1 (
	@echo 保存失败，请将当前界面截图后与捷信达联系
) else (
	@echo 保存成功
)