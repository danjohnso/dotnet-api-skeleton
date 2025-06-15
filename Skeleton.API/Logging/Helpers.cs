using System.Reflection;

namespace Skeleton.API.Logging
{
    public static class Helpers
    {
        private const string UnknownCaller = "Unknown Caller";

        /// <summary>
        /// Returns the application name from the entry assembly
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationName() => Assembly.GetEntryAssembly()?.GetName().Name ?? UnknownCaller;
    }
}
