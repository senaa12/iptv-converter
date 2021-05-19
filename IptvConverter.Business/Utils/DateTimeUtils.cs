using System;

namespace IptvConverter.Business.Utils
{
    public static class DateTimeUtils
    {
        public static DateTime GetZagrebCurrentDateTime()
        {
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, GetZagrebTimezone()).DateTime;
        }

        public static TimeZoneInfo GetZagrebTimezone()
        {
            return TimeZoneInfo.FindSystemTimeZoneById(false ? "Europe/Zagreb" : "Central European Standard Time");
        }
    }
}
