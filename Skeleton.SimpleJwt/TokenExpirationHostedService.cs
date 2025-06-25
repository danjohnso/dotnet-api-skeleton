using Skeleton.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Skeleton.SimpleJwt.Extensions;

namespace Skeleton.SimpleJwt
{
    public class TokenExpirationHostedService(ILogger<TokenExpirationHostedService> logger, IServiceScopeFactory serviceScopeFactory) 
        : TimerHostedService(logger, serviceScopeFactory, TimeSpan.FromMinutes(5))
    {
        protected override async Task RunJobAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation($"{nameof(TokenExpirationHostedService)} is starting to work");

            try
            {
                using IServiceScope scope = ServiceScopeFactory.CreateScope();

                AppUserStore userStore = scope.ServiceProvider.GetRequiredService<AppUserStore>();
                TokenService tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();

                List<IdentityUserToken<Guid>> mfaTokens = await userStore.Context.UserTokens.Where(x => x.LoginProvider == SimpleJwtConstants.Provider && x.Name == SimpleJwtConstants.MfaLoginTokenType).ToListAsync(stoppingToken);

                Logger.LogInformation("Checking {TokenCount} MFA tokens", mfaTokens.Count);

                foreach (IdentityUserToken<Guid> token in mfaTokens)
                {
                    if (token.Value.IsWhiteSpace() || !await tokenService.ValidateTokenAsync(token.Value, SimpleJwtConstants.MfaLoginTokenType))
                    {
                        userStore.Context.UserTokens.Remove(token);
                    }
                }

                await userStore.Context.SaveChangesAsync(stoppingToken);

                List<IdentityUserToken<Guid>> refreshTokens = await userStore.Context.UserTokens.Where(x => x.LoginProvider == SimpleJwtConstants.Provider && x.Name == SimpleJwtConstants.RefreshTokenType).ToListAsync(stoppingToken);

                Logger.LogInformation("Checking {TokenCount} refresh tokens", refreshTokens.Count);
                
                foreach (IdentityUserToken<Guid> token in refreshTokens)
                {
                    if (token.Value.IsWhiteSpace() || !await tokenService.ValidateTokenAsync(token.Value, SimpleJwtConstants.MfaLoginTokenType))
                    {
                        userStore.Context.UserTokens.Remove(token);
                    }
                }

                await userStore.Context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Unable to do {nameof(TokenExpirationHostedService)} work");
            }

            Logger.LogInformation($"{nameof(TokenExpirationHostedService)} is finished working");
        }
    }
}
