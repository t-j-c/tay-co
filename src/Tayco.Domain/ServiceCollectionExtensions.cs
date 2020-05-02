using Microsoft.Extensions.DependencyInjection;
using Tayco.Domain.Repositories;
using Tayco.Domain.Services;

namespace Tayco.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddSingleton<IBlogRepository, BlogRepository>();
            services.AddSingleton<BlogService>();
            return services;
        }
    }
}
