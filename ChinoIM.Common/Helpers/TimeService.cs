using System;

namespace ChinoIM.Common.Helpers
{
    public static class TimeService
    {
        private static DateTime unixTime = new DateTime(1970, 1, 1);
        public static long CurrentTime
        {
            get
            {
                var ts = DateTime.Now - unixTime;
                return (long)ts.TotalSeconds;
            }
        }
    }
}
