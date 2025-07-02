namespace Skeleton.SimpleJwt.Requests
{
    public class MfaRequest
    {
        public required string Token { get; set; }
        public required string Code { get; set; }
    }
}
