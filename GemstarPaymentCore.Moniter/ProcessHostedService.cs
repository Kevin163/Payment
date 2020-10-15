using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GemstarPaymentCore.Moniter
{
    /// <summary>
    /// 进程后台运行服务
    /// </summary>
    public class ProcessHostedService : IHostedService
    {
        private ILogger _logger;
        private ProcessOption _processOption;
        public ProcessHostedService(ILogger<ProcessHostedService> logger, IOptions<ProcessOption> processOption)
        {
            _logger = logger;
            _processOption = processOption.Value;
        }
        private Dictionary<int, ProcessItem> _processIdAndItemDic = new Dictionary<int, ProcessItem>();
        private List<Process> _processes = new List<Process>();
        /// <summary>
        /// 启动服务，启动时将自动启动配置中的所有进程
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var item in _processOption.Items)
            {
                StartProcess(item);
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// 启动进程任务
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private void StartProcess(ProcessItem item)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = item.FileName,
                WorkingDirectory = string.IsNullOrEmpty(item.WorkingDirectory) ? Directory.GetCurrentDirectory() : item.WorkingDirectory,
                Arguments = item.Arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;
            var startSuccess = process.Start();
            if (startSuccess)
            {
                _processIdAndItemDic.Add(process.Id, item);
                _processes.Add(process);
                process.EnableRaisingEvents = true;
                process.Exited += Process_Exited;
            } else
            {
                process.Dispose();
            }
            _logger.LogInformation($"启动{item.Name}进程成功，进程id:{process.Id}");
        }
        /// <summary>
        /// 进程退出时，重新启动进程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Process_Exited(object sender, EventArgs e)
        {
            var process = sender as Process;
            if (process != null)
            {
                _processes.Remove(process);
                _logger.LogError($"进程id:{process.Id}退出，退出代码:{process.ExitCode}，将自动重新启动");
                //异常退出时重新启动
                StartProcess(_processIdAndItemDic[process.Id]);
            }
        }
        /// <summary>
        /// 终止服务，需要把所有由此服务启动的进程停止掉
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            //正常退出时，需要先设置不触发事件，然后再关闭任务进程
            foreach (var process in _processes)
            {
                process.EnableRaisingEvents = false;
                var id = process.Id;
                process.Kill();
                process.Dispose();
                var item = _processIdAndItemDic[id];
                _logger.LogInformation($"程序正常退出，将同时退出{item.Name}进程，进程id:{id}");
            }
            return Task.CompletedTask;
        }
    }
}
