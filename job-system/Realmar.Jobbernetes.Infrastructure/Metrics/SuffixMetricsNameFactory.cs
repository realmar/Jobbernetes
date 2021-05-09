namespace Realmar.Jobbernetes.Infrastructure.Metrics
{
    public class SuffixMetricsNameFactory : IMetricsNameFactory
    {
        private readonly IMetricsNameFactory _factory;
        private readonly string              _suffix;

        public SuffixMetricsNameFactory(string suffix, IMetricsNameFactory factory)
        {
            _factory = factory;
            _suffix  = suffix;
        }

        public string Create(string name) => $"{_factory.Create(name)}_{_suffix}";
    }
}
