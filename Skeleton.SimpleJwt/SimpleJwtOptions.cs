namespace Skeleton.SimpleJwt
{
    public class SimpleJwtOptions
    {
        public required string Audience { get;set; }
        public required string Issuer { get; set; }
        public required string CurrentSigningKey { get; set; }
        /// <summary>
        /// Defaults to 30 
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 30;
        /// <summary>
        /// Defaults to 43200 (30 days)
        /// </summary>
        public int RefreshTokenExpirationMinutes { get; set; } = 43200;
        public string? PreviousSigningKey { get; set; }
    }
}
