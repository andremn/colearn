using System;

namespace FinalProject.Extensions
{
    public enum DateTimeAddMode
    {
        Days = 0,
        Weeks,
        Months
    }

    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixStartDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimeStamp(this DateTime dateTime)
        {
            return (int)dateTime.Subtract(UnixStartDateTime).TotalSeconds;
        }

        public static long ToUnixTimeStampMilliseconds(this DateTime dateTime)
        {
            return (long)dateTime.Subtract(UnixStartDateTime).TotalMilliseconds;
        }

        public static DateTime Add(this DateTime dateTime, int value, DateTimeAddMode mode)
        {
            switch (mode)
            {
                case DateTimeAddMode.Days:
                    return dateTime.AddDays(value);
                case DateTimeAddMode.Weeks:
                    return dateTime.AddDays(value*7);
                case DateTimeAddMode.Months:
                    return dateTime.AddMonths(value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static DateTime SetTimeOfDay(this DateTime dateTime, TimeSpan timeOfDay)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                timeOfDay.Hours,
                timeOfDay.Minutes,
                timeOfDay.Seconds);
        }

        public static DateTime SetDate(this DateTime dateTime, DateTime date)
        {
            return new DateTime(
                date.Year,
                date.Month,
                date.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                date.Kind);
        }

        public static DateTime SetMonth(this DateTime dateTime, int month)
        {
            if (month <= 0 || month > 12)
            {
                throw new ArgumentException("Invalid month.");
            }

            return new DateTime(
                dateTime.Year,
                month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second);
        }
    }
}