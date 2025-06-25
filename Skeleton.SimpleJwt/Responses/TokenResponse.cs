namespace Skeleton.SimpleJwt.Responses
{
    public record TokenResponse(SimpleToken AccessToken, SimpleToken RefreshToken);
}
