using System;
using CSOMTimeZone = Microsoft.SharePoint.Client.TimeZone;

namespace Bugfree.Spo.Cqrs.Core.Extensions
{
    public class TimeZoneExtensions
    {
        public DateTime ToLocalTime(CSOMTimeZone tz, DateTime dt)
        {
            return dt.AddMinutes(-tz.Information.Bias - tz.Information.DaylightBias);
        }
    }
}
