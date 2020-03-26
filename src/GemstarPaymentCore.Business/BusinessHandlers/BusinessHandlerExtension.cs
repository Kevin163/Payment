using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GemstarPaymentCore.Business.BusinessHandlers
{
    /// <summary>
    /// 业务处理类注册
    /// </summary>
    public static class BusinessHandlerExtension
    {
        public static IServiceCollection AddBusinessHandlers(this IServiceCollection services)
        {
            var assembly = Assembly.GetAssembly(typeof(IBusinessHandler));
            if (assembly != null)
            {
                var types = assembly.GetTypes();
                var interfaceName = typeof(IBusinessHandler).Name;
                foreach(var type in types)
                {
                    var interfaceType = type.GetInterface(interfaceName);
                    if(interfaceType != null)
                    {
                        if (!type.IsAbstract)
                        {
                            services.AddScoped(type);
                        }
                    }
                }
            }
            return services;
        }
    }
}
