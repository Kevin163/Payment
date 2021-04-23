using Microsoft.AspNetCore.SignalR;
using System;
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
        /// <param name="imageUrl">the pdf file first page's image url</param>
        /// <returns></returns>
        public async Task ShowRC(string deviceId, string pdfFileName, string qrcode, string regid, string amount,string imageUrl,int needInputPhoneNo,string defaultPhoneNo)
        {
            var group = GetGroup(deviceId);
            await group.SendAsync("ShowRC", regid, pdfFileName, qrcode, amount,imageUrl,needInputPhoneNo.ToString(),defaultPhoneNo);
        }
        /// <summary>
        /// show the payment qrcode again
        /// </summary>
        /// <param name="deviceId">the android device id</param>
        /// <param name="regid">regid fro guest checkin</param>
        /// <returns></returns>
        public async Task RePay(string deviceId,string regid)
        {
            var group = GetGroup(deviceId);
            await group.SendAsync("ShowPayQrcode", regid);
        }
        /// <summary>
        /// show the home page when user payment successed
        /// </summary>
        /// <param name="deviceId">the android device id</param>
        /// <param name="regid">regid for guest checkin</param>
        /// <param name="seconds">the seconds of show payment result</param>
        /// <returns></returns>
        public async Task ShowHome(string deviceId,string regid,int seconds)
        {
            var group = GetGroup(deviceId);
            await group.SendAsync("ShowHome", regid,seconds);
        }
        /// <summary>
        /// get the group for the device id
        /// </summary>
        /// <param name="deviceId">the device id</param>
        /// <returns>the group contains the device id</returns>
        private IClientProxy GetGroup(string deviceId)
        {
            try
            {
                var group = Clients.Group(deviceId);
                if(group == null)
                {
                    throw new ArgumentException("device id is wrong or the device is not online");
                }
                return group;
            }
            catch (Exception)
            {
                throw new ArgumentException("device id is wrong or the device is not online");
            }
        }
    }
}
