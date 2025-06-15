using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skeleton.Email.Interfaces;
using Skeleton.Email.Services;

namespace Skeleton.Email
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
            services.AddTransient<IMailService, SmtpService>();

            return services;
        }
    }
}
