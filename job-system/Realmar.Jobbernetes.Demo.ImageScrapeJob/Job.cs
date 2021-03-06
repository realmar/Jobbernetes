using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Demo.ImageScrapeJob.Exceptions;
using Realmar.Jobbernetes.Demo.ImageScrapeJob.Options;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Infrastructure.Jobs;
using Realmar.Jobbernetes.Infrastructure.Messaging;

namespace Realmar.Jobbernetes.Demo.ImageScrapeJob
{
    internal class Job : IJob<ImageInput>
    {
        private readonly HttpClient                  _httpClient;
        private readonly IOptions<DemoOptions>       _options;
        private readonly IQueueProducer<ImageOutput> _producer;
        private readonly Random                      _random = new();
        private readonly string                      _url;

        public Job(HttpClient                       httpClient,
                   IQueueProducer<ImageOutput>      producer,
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
        public async Task ProcessAsync(ImageInput data, CancellationToken cancellationToken)
        {
            static int Clamp(DemoOptions.Range range, Func<int, int, int> reducer) =>
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

            var response = await _httpClient
                                .GetAsync($"{_url}/{_options.Value.TextPrefix}{data.Name}{_options.Value.TextPostfix}",
                                          cancellationToken)
                                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

            await _producer.ProduceAsync(new(DateTime.UtcNow, data.Name, Convert.ToBase64String(bytes)), cancellationToken)
                           .ConfigureAwait(false);
        }
    }
}
