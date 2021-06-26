using Realmar.Jobbernetes.AdminWeb.Shared.Primitives;

namespace Realmar.Jobbernetes.AdminWeb.Shared.Models
{
    public record ProcessingMetrics(Count Count, Throughput Throughput)
    {
        public ProcessingMetrics() : this(0, 0) { }

        public static ProcessingMetrics operator +(ProcessingMetrics a, ProcessingMetrics b)
            => new(a.Count + b.Count, a.Throughput + b.Throughput);
    }
}
