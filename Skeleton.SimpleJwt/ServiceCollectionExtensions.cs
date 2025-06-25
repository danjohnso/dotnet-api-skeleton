using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Skeleton.SimpleJwt
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleJwt(this IServiceCollection services, IConfiguration configuration, string configurationSectionName = "Jwt", Action<JwtBearerOptions>? configureOptions = null)
        {
            IConfigurationSection jwtSection = configuration.GetRequiredSection(configurationSectionName);

            SimpleJwtOptions jwtOptions = jwtSection.Get<SimpleJwtOptions>()
                ?? throw new InvalidOperationException($"Configuration section '{configurationSectionName}' is missing or invalid.");

            services.Configure<SimpleJwtOptions>(jwtSection);
            services.AddMemoryCache();
            services.AddScoped<TokenService>();

            //cleanup invalid UserTokens
            services.AddHostedService<TokenExpirationHostedService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                if (configureOptions is not null)
                {
                    configureOptions(options);
                }

                options.Authority = jwtOptions.Issuer;
                options.Audience = jwtOptions.Audience;
                options.MapInboundClaims = false;

                List<SymmetricSecurityKey> keys = [
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.CurrentSigningKey))
                ];

                if (keys[0].KeySize < 256)
                {
                    throw new InvalidOperationException("'CurrentSigningKey' must be at least 256 bits.");
                }

                if (jwtOptions.PreviousSigningKey is not null)
                {
                    keys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.PreviousSigningKey)));

                    if (keys[1].KeySize < 256)
                    {
                        throw new InvalidOperationException("'PreviousSigningKey' must be at least 256 bits.");
                    }
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    IssuerSigningKeys = keys
                };
            });

            return services;
        }
    }
}
