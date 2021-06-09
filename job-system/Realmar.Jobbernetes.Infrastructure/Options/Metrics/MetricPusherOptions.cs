namespace Realmar.Jobbernetes.Infrastructure.Options.Metrics
{
    public class MetricPusherOptions
    {
        public string Endpoint { get; set; } = "http://localhost:9091/metrics";
        public string Job      { get; set; } = "jobbernetes_jobs";
        public string JobName  { get; set; } = "job_name_00";

        internal string[] GetLabelValues() => new[] { JobName };
    }
}
