using Microsoft.Extensions.Logging;

namespace Globals
{
    public static class globalVariables
    {
        public static int Units_in_M = 1000;
        public static int seconds_per_timestep = 60;
        public const long M_in_Lsecond = 299792458;
        public static  Microsoft.Extensions.Logging.LogLevel Level = Microsoft.Extensions.Logging.LogLevel.Trace;
        public static ILogger log = new Logger().log;
    }

    public class Logger
    {
        public ILogger log { get; private set; }
        public static Microsoft.Extensions.Logging.LogLevel Level = Microsoft.Extensions.Logging.LogLevel.Trace;

        public Logger() {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(); // Logs will appear in the console
                builder.AddFilter("Program", Level);
            });

            // Create a logger
            log = loggerFactory.CreateLogger<Program>();        
        }

    }

}
