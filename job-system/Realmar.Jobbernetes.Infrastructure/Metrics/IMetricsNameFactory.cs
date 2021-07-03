namespace Realmar.Jobbernetes.Infrastructure.Metrics
{
    /// <summary>
    ///     Generate the metric keys.
    /// </summary>
    public interface IMetricsNameFactory
    {
        /// <summary>
        ///     Creates a metric key given the name.
        /// </summary>
        /// <param name="name">The name.</param>
        string Create(string name);
    }
}
