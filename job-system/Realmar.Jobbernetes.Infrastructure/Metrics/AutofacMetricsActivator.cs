using System;
using Autofac;
using Microsoft.Extensions.Options;
using Prometheus;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Infrastructure.Metrics
{
    public class AutofacMetricsActivator : IStartable, IDisposable
    {
        private readonly IOptions<MetricsOptions> _options;
        private readonly IMetricServer            _server;

        public AutofacMetricsActivator(IMetricServer server, IOptions<MetricsOptions> options)
        {
            _server  = server;
            _options = options;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _server.Dispose();
        }

        public void Start()
        {
            var instanceName = _options.Value.InstanceName;
            if (string.IsNullOrEmpty(instanceName) == false)
            {
                Prometheus.Metrics.DefaultRegistry.SetStaticLabels(new() { ["instance"] = instanceName });
            }

            _server.Start();
        }
    }
}
