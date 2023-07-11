using System.Reflection;
using AccountManagement.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using AccountManagement.Application.Common.Behavior;
using MediatR;
using AccountManagement.Application.Redis.Common;

namespace AccountManagement.Application
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            // system config
            
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddSingleton<IQuery, SqlServer>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddSingleton<IRedisProvider, RedisProvider>();

            return services;
        }
    }
}
