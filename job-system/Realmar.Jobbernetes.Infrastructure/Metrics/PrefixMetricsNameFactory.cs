namespace Realmar.Jobbernetes.Infrastructure.Metrics
{
    public class PrefixMetricsNameFactory : IMetricsNameFactory
    {
        private readonly IMetricsNameFactory _factory;
        private readonly string              _prefix;

        public PrefixMetricsNameFactory(string prefix, IMetricsNameFactory factory)
        {
            _factory = factory;
            _prefix  = prefix;
        }

        public string Create(string name) => $"{_factory.Create($"{_prefix}_{name}")}";
    }
}
