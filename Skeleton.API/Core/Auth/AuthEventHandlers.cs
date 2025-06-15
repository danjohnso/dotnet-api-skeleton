using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace Skeleton.API.Core.Auth
{
    internal static class AuthEventHandlers
    {
        public static Task OnTokenValidatedHandler(TokenValidatedContext context)
        {
            ClaimsIdentity? claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
            claimsIdentity?.AddClaim(new Claim("scheme", context.Scheme.Name));

            return Task.CompletedTask;
        }
    }
}