using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Demo.Infrastructure.Exceptions;
using Realmar.Jobbernetes.Demo.Infrastructure.Options;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;

namespace Realmar.Jobbernetes.Demo.ScraperJobService
{
    internal class Job : IJob<ImageIngress>
    {
        private readonly HttpClient            _httpClient;
        private readonly IOptions<DemoOptions> _options;
        private readonly IQueueProducer<Image> _producer;
        private readonly Random                _random = new();
        private readonly string                _url;

        public Job(HttpClient                       httpClient,
                   IQueueProducer<Image>            producer,
                   IOptions<ExternalServiceOptions> externalServiceOptions,
                   IOptions<DemoOptions>            demoOptions)
        {
            _httpClient = httpClient;
            _producer   = producer;
            _options    = demoOptions;
            _url        = externalServiceOptions.Value.Url;
        }

        /// <exception cref="T:Realmar.Jobbernetes.Demo.Infrastructure.Exceptions.DemoException">
        ///     Synthetic error in demo to simulate a failure
        /// </exception>
        public async Task Process(ImageIngress data, CancellationToken cancellationToken)
        {
            int Clamp(DemoOptions.Range range, Func<int, int, int> reducer) =>
                Math.Clamp(reducer.Invoke(range.Min, range.Max), 0, int.MaxValue);

            var delay = _options.Value.ProcessingDelayMilliseconds;
            if (delay.Max > 0)
            {
                var min    = Clamp(delay, Math.Min);
                var max    = Clamp(delay, Math.Max);
                var actual = _random.Next(min, max);

                await Task.Delay(TimeSpan.FromMilliseconds(actual), cancellationToken).ConfigureAwait(false);
            }

            var failureProbability = _options.Value.FailureProbability;
            if (_random.NextDouble() <= failureProbability)
            {
                throw new DemoException("Synthetic error in demo to simulate a failure");
            }

            var response = await _httpClient.GetAsync($"{_url}/{data.Name}", cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

            await _producer.ProduceAsync(new(data.Name, Convert.ToBase64String(bytes)), cancellationToken)
                           .ConfigureAwait(false);
        }
    }
}
