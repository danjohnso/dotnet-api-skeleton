using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skeleton.Business.Repositories;
using Skeleton.Business.Services;
using Skeleton.Data;
using Skeleton.Email;

namespace Skeleton.Business
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusiness(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.Configure<AzureStorageOptions>(configuration.GetSection("AzureStorage"));

            services.AddTransient<FileService>();
            services.AddTransient<UserService>();

            services.AddTransient<AzureStorageRepository>();

            services.AddDataContext(configuration);
            services.AddEmail(configuration);

            return services;
        }
    }
}
