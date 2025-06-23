using Skeleton.SimpleJwt.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Skeleton.SimpleJwt
{
    public static class TokenServiceEndpoints
    {
        /// <summary>
        /// Adds the token service endpoints under the specified prefix: 'login', 'mfa', 'refresh', 'logout'
        /// </summary>
        /// <param name="app"></param>
        /// <param name="routePrefix"></param>
        public static void AddSimpleJwtEndpoints(this WebApplication app, string routePrefix = "/token")
        {
            if (routePrefix == "/")
            {
                routePrefix = "";
            }

            if (!routePrefix.StartsWith('/'))
            {
                routePrefix = "/" + routePrefix;
            }

            app.MapPost($"{routePrefix}/login", async (LoginRequest request, TokenService service) => await service.LoginAsync(request));
            app.MapPost($"{routePrefix}/mfa", async (MfaRequest request, TokenService service) => await service.MfaAsync(request));
            app.MapPost($"{routePrefix}/refresh", async (RefreshRequest request, TokenService service) => await service.RefreshAsync(request));
            app.MapPost($"{routePrefix}/logout", async (HttpContext context, TokenService service) => await service.LogoutAsync(context)).RequireAuthorization();
        }
    }
}
