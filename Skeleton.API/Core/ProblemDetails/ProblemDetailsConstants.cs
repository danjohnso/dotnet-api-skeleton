namespace Skeleton.API.Core.ProblemDetails
{
    public static class ProblemDetailsConstants
    {
        public const string TraceIdKey = "traceId";
        public const string ErrorsKey = "errors";
        public const string ContentType = "application/problem+json";

        public const string Status400Uri = "https://www.ietf.org/rfc/rfc9110.html#section-15.5.1";
        public const string Status404Uri = "https://www.ietf.org/rfc/rfc9110.html#section-15.5.5";
        public const string Status409Uri = "https://www.ietf.org/rfc/rfc9110.html#section-15.5.10";
        public const string Status422Uri = "https://www.ietf.org/rfc/rfc9110.html#section-15.5.21";
        public const string Status500Uri = "https://www.ietf.org/rfc/rfc9110.html#section-15.6.1";
    }
}
