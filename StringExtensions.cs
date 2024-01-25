using System;
using System.Globalization;

namespace Gatekeeper
{
    public static class StringExtensions
    {
        public static string ToShortString(this TimeSpan span)
        {
            double runTime = span.TotalMilliseconds;

            if (runTime < 1000)
                return $"{span.Milliseconds}ms";
            if (runTime < 60000)
                return string.Format("{0:0.0}s", runTime / 1000.0, CultureInfo.InvariantCulture);
            if (runTime < 3600000)
                return string.Format("{0:0.0}m", runTime / 60000.0, CultureInfo.InvariantCulture);
            if (runTime < 86400000)
                return string.Format("{0:0.0}h", runTime / 3600000.0, CultureInfo.InvariantCulture);
            return string.Format("{0:0.0}d", runTime / 86400000.0, CultureInfo.InvariantCulture);
        }
    }
}
