namespace Realmar.Jobbernetes.Framework.Options
{
    public class MetricServerOptions
    {
        public string Hostname { get; set; } = "localhost";

        // https://github.com/prometheus/prometheus/wiki/Default-port-allocations
        public int Port { get; set; } = 9098;
    }
}
