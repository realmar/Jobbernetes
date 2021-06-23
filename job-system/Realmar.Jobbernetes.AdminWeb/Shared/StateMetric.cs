using System;

namespace Realmar.Jobbernetes.AdminWeb.Shared
{
    public record StateMetric(int Count, int Throughput)
    {
        public static StateMetric operator +(StateMetric a, StateMetric b)
            => new(a.Count + b.Count, a.Throughput + b.Throughput);

        private string FormatNumber(int number) => number > 1000 ? Math.Round(number / 1000d) + "k" : number.ToString();

        public override string ToString() => $"{FormatNumber(Count)} ({FormatNumber(Throughput)}/s)";
    }
}
