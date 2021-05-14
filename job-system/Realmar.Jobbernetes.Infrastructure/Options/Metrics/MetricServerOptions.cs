namespace Realmar.Jobbernetes.Framework.Options.Metrics
{
    public class MetricServerOptions
    {
        public string Hostname { get; set; } = "localhost";

        // https://github.com/prometheus/prometheus/wiki/Default-port-allocations
        public int    Port { get; set; } = 9098;
        public string Path { get; set; } = "/metrics";
    }
}
