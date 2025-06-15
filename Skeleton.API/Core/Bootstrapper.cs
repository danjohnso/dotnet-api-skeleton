using Serilog;
using Serilog.Debugging;

namespace Skeleton.API.Core
{
    public static class Bootstrapper
    {
        private const string BootstrapFile = "bootstrap.txt";

        public static void Run(string[] args, Func<WebApplicationBuilder> builder, Action<WebApplication> hostConfiguration)
        {
            if (File.Exists(BootstrapFile))
            {
                File.Delete(BootstrapFile);
            }

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(BootstrapFile)
                .CreateBootstrapLogger();

            Log.Information("Bootstrapping...");

            if (args.Length > 0 && args.Contains("-bootstrap"))
            {
                Log.Information("Self Log enabled");
                SelfLog.Enable(Console.Error);
            }

            try
            {
                Log.Information("Creating WebApplicationBuilder");

                WebApplicationBuilder hostBuilder = builder();

                Log.Information("WebApplicationBuilder ready, building host");

                //No logs after this point will actually write with Log
                //because the runtime logger will be configured which will replace this
                //Fatal exceptions will still write since the logger should not be replaced yet

                WebApplication host = hostBuilder.Build();

                hostConfiguration?.Invoke(host);

                host.Run();
            }
            catch (Exception ex)
            {
                //ignoring this excetion type for the EF tooling
                if (ex is not HostAbortedException)
                {
                    Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
                }
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
