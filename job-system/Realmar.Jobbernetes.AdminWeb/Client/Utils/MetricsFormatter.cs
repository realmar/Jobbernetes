using System;
using Realmar.Jobbernetes.AdminWeb.Shared.Primitives;

namespace Realmar.Jobbernetes.AdminWeb.Client.Formatters
{
    internal static class MetricsFormatter
    {
        private const int RoundDecimals = 2;

        public static string Format(this Count self)
        {
            if (self > 1000)
            {
                return $"{Math.Round((double) self / 1000d, RoundDecimals)}k";
            }

            return self.ToString();
        }

        public static string Format(this Throughput self)
        {
            return $"{self}/s";
        }

        public static string Format(this Percentage self)
        {
            return $"{Math.Round((double) self, RoundDecimals):0.##}%";
        }
    }
}
