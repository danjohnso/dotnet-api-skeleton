namespace Skeleton.API.Core
{
    public static class HostEnvironmentExtensions
    {
        public const string Local = "Local";

        public static bool IsLocal(this IHostEnvironment hostEnvironment)
        {
            return hostEnvironment.IsEnvironment(Local);
        }

        public static bool IsDevelopmentOrLocal(this IHostEnvironment hostEnvironment)
        {
            return hostEnvironment.IsEnvironment(Local) || hostEnvironment.IsDevelopment();
        }
    }
}
