using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Skeleton.Identity.Entities;

namespace Skeleton.Identity
{
	public static class IdentityDatabaseRegistration
    {
        /// <summary>
        /// Adds references to AppIdentityContext
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppIdentityData(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<AppIdentityContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(nameof(AppIdentityContext)), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                    sqlOptions.MigrationsAssembly(typeof(AppIdentityContext).Assembly.GetName().Name);
                });
            });

            return services;
        }

        /// <summary>
        /// Adds everything from AddAppIdentityData, plus pieces for using the AppUserManager
        /// This does NOT run the default AddIdentity, just a subset of it
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IdentityBuilder AddAppIdentityManagement(this IServiceCollection services, IConfiguration configuration)
        {
            //add the database connection
            services.AddAppIdentityData(configuration);

            //set options
            services.Configure<PasswordHasherOptions>(options => { options.IterationCount = AppIdentityOptions.HashIterationCount; });

            //return the builder
            return services.AddIdentityCore<User>(AppIdentityOptions.DefaultIdentitySetup)
                        .AddEntityFrameworkStores<AppIdentityContext>()
                        .AddUserManager<AppUserManager>()
                        .AddSignInManager<AppSignInManager>()
                        .AddUserStore<AppUserStore>()
                        .AddDefaultTokenProviders()
                        .AddPasswordValidator<RepeatingCharacterPasswordValidator<User>>();
        }
    }
}