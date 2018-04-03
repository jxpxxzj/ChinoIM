using Microsoft.Extensions.Logging;
using System;

namespace ChinoIM.Common.Helpers
{
    public static class LogManager
    {
        private static ILoggerFactory loggerFactory = new LoggerFactory().AddConsole().AddDebug();

        public static ILogger CreateLogger<T>()
        {
            return loggerFactory.CreateLogger<T>();
        }

        public static ILogger CreateLogger(Type type)
        {
            return loggerFactory.CreateLogger(type);
        }

        public static ILogger CreateLogger<T>(string msg)
        {
            return loggerFactory.CreateLogger(typeof(T).FullName + msg);
        }
    }
}
