echo "本程序将安装捷信达支付中间件为paymentService服务。\n如果安装过程中出现错误，请以管理员的身份来运行"
sc create paymentService binPath= "%cd%\GemstarPaymentCore.exe"
sc start paymentService