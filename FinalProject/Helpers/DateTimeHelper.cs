using System;

namespace FinalProject.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly DateTime UnixStartDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTimeStamp(long seconds)
        {
            return UnixStartDateTime.AddSeconds(seconds);
        }

        public static DateTime FromUnixTimeStampMilliseconds(long milliseconds)
        {
            return UnixStartDateTime.AddMilliseconds(milliseconds);
        }

        public static long GetNowUnixTimeStamp()
        {
            return (int)DateTime.UtcNow.Subtract(UnixStartDateTime).TotalSeconds;
        }

        public static long GetNowUnixTimeStampMilliseconds()
        {
            return (long)DateTime.UtcNow.Subtract(UnixStartDateTime).TotalMilliseconds;
        }
    }
}