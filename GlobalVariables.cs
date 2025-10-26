using Microsoft.Extensions.Logging;

namespace Globals
{
    public static class globalVariables
    {
        public static int Units_in_M = 1000;
        public static int seconds_per_timestep = 60;
        public static decimal max_angle_initialisation = (decimal)1;

        public static double min_massive_weight = 100000000;
        public const long M_in_Lsecond = 299792458;
        public const decimal grav_const = (decimal)0.000000000066743;
        public static  Microsoft.Extensions.Logging.LogLevel Level = Microsoft.Extensions.Logging.LogLevel.Information;
        public static ILogger log = new Logger().log;

        public static List<WaitHandle> waitHandles = new List<WaitHandle>();

        public static int threads = 6;
    }

    public class Logger
    {
        public ILogger log { get; private set; }
        public static Microsoft.Extensions.Logging.LogLevel Level = Microsoft.Extensions.Logging.LogLevel.Information;

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
