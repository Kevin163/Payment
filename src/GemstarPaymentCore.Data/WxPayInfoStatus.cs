namespace GemstarPaymentCore.Data
{
    /// <summary>
    /// 微信支付中间表记录状态
    /// </summary>
    public enum WxPayInfoStatus : int
    {
        /// <summary>
        /// 新单，刚由业务系统生成，还没有将此订单信息传递到阿里云上面
        /// </summary>
        New = 0,
        /// <summary>
        /// 已上传，已经将此订单信息传递给阿里云，等待用户扫码进行支付
        /// </summary>
        UploadOnly = 1,
        /// <summary>
        /// 已处理，用户已经扫码，微信已经回调此订单信息，已经生成预支付交易号，等待支付
        /// </summary>
        WaitPay = 2,
        /// <summary>
        /// 支付成功，已经支付成功
        /// </summary>
        PaidSuccess = 3,
        /// <summary>
        /// 支付失败
        /// </summary>
        PaidFailure = 4,
        /// <summary>
        /// 撤单，由业务系统发起，发起前需要检查是否可以撤单，阿里云检测到此状态需要关闭对应的预支付交易
        /// </summary>
        Cancel = 5,
        /// <summary>
        /// 易宝支付的新单
        /// </summary>
        NewForYeePay = 10,
        /// <summary>
        /// 智能pos支付的新单
        /// </summary>
        NewForSmartPos = 20,
        /// <summary>
        /// 智能pos待退款
        /// </summary>
        WaitRefundForSmartPos = 21,
        /// <summary>
        /// 智能pos已退款
        /// </summary>
        RefundedForSmartPos = 22,
        /// <summary>
        /// 微信服务商支付的新单，不需要上传到云端，只需要直接从微信服务器上查询支付状态即可
        /// </summary>
        NewForWxProviderPay = 30,
        /// <summary>
        /// 利楚商务扫呗支付的新单
        /// </summary>
        NewForLcswPay = 40,
        /// <summary>
        /// 捷信达聚合支付的新单
        /// </summary>
        NewForJxdUnionPay = 50,
    }
}
