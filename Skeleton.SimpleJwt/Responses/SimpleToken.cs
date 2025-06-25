namespace Skeleton.SimpleJwt.Responses
{
    public record SimpleToken(string Token, long ExpiresIn);
}
