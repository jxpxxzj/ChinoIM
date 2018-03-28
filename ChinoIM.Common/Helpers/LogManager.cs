using Microsoft.Extensions.Logging;

namespace ChinoIM.Common.Helpers
{
    public static class LogManager
    {
        private static ILoggerFactory loggerFactory = new LoggerFactory().AddConsole().AddDebug();

        public static ILogger CreateLogger<T>()
        {
            return loggerFactory.CreateLogger<T>();
        }

        public static ILogger CreateLogger<T>(string msg)
        {
            return loggerFactory.CreateLogger(typeof(T).FullName + msg);
        }
    }
}
