using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Models
{
    public class RCSignHub : Hub
    {
        /// <summary>
        /// 绑定当前连接到指定的设备编号上
        /// 需要在连接建立后，自动执行一下此方法来进行绑定
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <returns></returns>
        public async Task BindDeviceId(string deviceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, deviceId);
        }
        /// <summary>
        /// show the rc pdf in the android device
        /// </summary>
        /// <param name="deviceId">the android device id</param>
        /// <param name="pdfFileName">pdf file url</param>
        /// <param name="qrcode">pay qrcode content</param>
        /// <param name="regid">the guest checkin id </param>
        /// <param name="amount">the payment amount</param>
        /// <returns></returns>
        public async Task ShowRC(string deviceId, string pdfFileName, string qrcode, string regid, string amount)
        {
            await Clients.Group(deviceId).SendAsync("ShowRC", regid, pdfFileName, qrcode, amount);
        }
        /// <summary>
        /// show the home page when user payment successed
        /// </summary>
        /// <param name="deviceId">the android device id</param>
        /// <param name="regid">regid for guest checkin</param>
        /// <returns></returns>
        public async Task ShowHome(string deviceId,string regid)
        {
            await Clients.Group(deviceId).SendAsync("ShowHome", regid);
        }
    }
}
