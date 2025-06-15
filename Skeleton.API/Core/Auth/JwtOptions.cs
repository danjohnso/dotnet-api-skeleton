namespace Skeleton.API.Core.Auth
{
    public class JwtOptions
    {
        public required string Audience { get;set; }
        public required string Authority { get; set; }
    }
}
