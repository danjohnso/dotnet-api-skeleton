namespace Skeleton.SimpleJwt.Requests
{
    public class MfaRequest
    {
        public required string EmailAddress { get; set; }
        public required string Code { get; set; }
    }
}
