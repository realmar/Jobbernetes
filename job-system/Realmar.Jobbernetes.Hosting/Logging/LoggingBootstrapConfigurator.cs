using System;
using Serilog;
using Serilog.Debugging;

namespace Realmar.Jobbernetes.Hosting.Logging
{
    public static class LoggingBootstrapConfigurator
    {
        public static void Configure()
        {
            var enableSelfLog = Environment.GetEnvironmentVariable("SerilogOptions__EnableSerilogDebugging");

            bool IsEnabled(string symbol) => string.Equals(enableSelfLog, symbol, StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(enableSelfLog) == false
             && (IsEnabled("true") || IsEnabled("yes") || IsEnabled("enable")))
            {
                SelfLog.Enable(Console.Error);
            }

            Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .CreateBootstrapLogger();
        }
    }
}
