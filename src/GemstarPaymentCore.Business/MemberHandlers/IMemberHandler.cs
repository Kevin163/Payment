using System.Collections.Generic;
using System.Threading.Tasks;

namespace GemstarPaymentCore.Business.MemberHandlers
{
    /// <summary>
    /// 会员接口
    /// </summary>
    public interface IMemberHandler
    {
        /// <summary>
        /// 使用微信openId查询对应的会员信息
        /// </summary>
        /// <param name="openId">微信openid</param>
        /// <returns>对应的会员信息，如果没有会员则为null</returns>
        Task<List<MemberInfo>> QueryMember(string openId);
        /// <summary>
        /// 会员交易，扣减会员储值余额
        /// </summary>
        /// <param name="para">交易参数实例</param>
        /// <returns>支付结果实例</returns>
        Task<MemberPaymentResult> MemberPayment(MemberPaymentParameter para);
        /// <summary>
        /// 查询会员券明细列表
        /// </summary>
        /// <param name="para">查询参数</param>
        /// <returns>券明细列表</returns>
        Task<List<MemberTicketInfo>> QueryMemberTickets(MemberTicketQueryParameter para);
        /// <summary>
        /// 会员券核销使用
        /// </summary>
        /// <param name="para">会员券使用参数</param>
        /// <returns>会员券使用结果</returns>
        Task<MemberPaymentResult> MemberUseTicket(MemberTicketUseParameter para);
        /// <summary>
        /// 会员券反核销
        /// </summary>
        /// <param name="para">反核销参数</param>
        /// <returns>反核销结果</returns>
        Task<MemberPaymentResult> MemberUnusedTicket(MemberTicketUnusedParameter para);
    }
}
