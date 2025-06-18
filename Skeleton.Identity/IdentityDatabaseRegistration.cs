using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        /// <param name="identityDbConnectionString"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppIdentityData(this IServiceCollection services, string identityDbConnectionString)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<AppIdentityContext>(options =>
            {
                options.UseNpgsql(identityDbConnectionString, sqlOptions =>
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
        /// <param name="identityDbConnectionString"></param>
        /// <returns></returns>
        public static IdentityBuilder AddAppIdentityManagement(this IServiceCollection services, string identityDbConnectionString)
        {
            //add the database connection
            services.AddAppIdentityData(identityDbConnectionString);

            // Identity services
            //services.TryAddScoped<IUserValidator<User>, UserValidator<User>>();
            //services.TryAddScoped<IPasswordValidator<User>, PasswordValidator<User>>();
            //services.TryAddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            //services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            //services.TryAddScoped<IRoleValidator<User>, RoleValidator<User>>();
            //services.TryAddScoped<IdentityErrorDescriber>();
            //services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<User>>();
            //services.TryAddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User>>();
            //services.TryAddScoped<UserManager<User>, AspNetUserManager<User>>();
            //services.TryAddScoped<SignInManager<User>, SignInManager<User>>();
            
            //no roles
            //services.TryAddScoped<RoleManager<Role>, AspNetRoleManager<Role>>();

            //set options
            services.Configure(AppIdentityOptions.DefaultIdentitySetup);
	        services.Configure<PasswordHasherOptions>(options => { options.IterationCount = AppIdentityOptions.HashIterationCount; });

            //return the builder
            return new IdentityBuilder(typeof(User), services)
                        .AddEntityFrameworkStores<AppIdentityContext>()
                        .AddUserManager<AppUserManager>()
						.AddSignInManager<AppSignInManager>()
                        .AddUserStore<AppUserStore>()
                        .AddDefaultTokenProviders()
						.AddPasswordValidator<RepeatingCharacterPasswordValidator<User>>();
        }
    }
}