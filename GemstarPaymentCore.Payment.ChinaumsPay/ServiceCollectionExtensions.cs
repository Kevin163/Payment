using Microsoft.Extensions.DependencyInjection;

namespace GemstarPaymentCore.Payment.ChinaumsPay
{
    public static class ServiceCollectionExtensions
    {
        public static void AddChinaumsPay(
            this IServiceCollection services)
        {
            services.AddScoped<IChinaumsPayClient, ChinaumsPayClient>();
        }

    }
}
