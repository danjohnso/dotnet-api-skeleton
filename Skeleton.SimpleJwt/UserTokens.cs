namespace Skeleton.SimpleJwt
{
    public readonly struct UserTokens(string accessToken, string refreshToken)
    {
        public readonly string AccessToken = accessToken;
        public readonly string RefreshToken = refreshToken;
    }
}
