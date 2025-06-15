using Serilog;
using System.Diagnostics;
using System.Reflection;

namespace Skeleton.API.Logging
{
    public static class HostingExtensions
    {
        const string DefaultSearchNamespace = "Skeleton.";

        /// <summary>
        /// Setup logging within the host builder
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureLogging(this IHostBuilder host)
        {
            return host.UseSerilog((hostingContext, services, logging) =>
            {
                logging.ReadFrom.Configuration(hostingContext.Configuration);
                logging.WriteTo.Console();
            });
        }
    }
}
