using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Infrastructure.Metrics
{
    internal class MetricsNameFactory : IMetricsNameFactory
    {
        private readonly IOptions<MetricsOptions> _options;

        public MetricsNameFactory(IOptions<MetricsOptions> options) => _options = options;

        public string Create(string name) => $"{_options.Value.BasePrefix}_{name}";
    }
}
