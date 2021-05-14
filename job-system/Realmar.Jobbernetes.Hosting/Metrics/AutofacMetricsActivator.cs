using System;
using Autofac;
using Prometheus.Client.MetricServer;

namespace Realmar.Jobbernetes.Infrastructure.Metrics
{
    internal class AutofacMetricsActivator : IStartable, IDisposable
    {
        private readonly IMetricServer _server;

        public AutofacMetricsActivator(IMetricServer server)
        {
            _server = server;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _server.Stop();
        }

        public void Start()
        {
            _server.Start();
        }
    }
}
