using System.Security.Claims;

namespace Skeleton.API.Core.Auth
{
    //https://auth0.com/docs/get-started/apis/scopes/sample-use-cases-scopes-and-claims
    public static class ClaimsExtensions
    {
        public const string CustomClaimNamespace = "https://api.skeleton.com";

        public static Guid GetId(this ClaimsPrincipal principal) => new(principal.Claims.Single(x => x.Type == $"{CustomClaimNamespace}/sub").Value);
    }
}
