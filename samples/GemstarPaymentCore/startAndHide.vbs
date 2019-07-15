Set ws = CreateObject("Wscript.Shell") 
ws.run "cmd /c start.bat",0 
msgbox "程序已经运行，可以从进程中查看dotnet.exe进程"
