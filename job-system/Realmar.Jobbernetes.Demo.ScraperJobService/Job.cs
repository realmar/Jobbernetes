using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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

        public async Task Process(ImageIngress data, CancellationToken cancellationToken)
        {
            var delay = _options.Value.ProcessingDelaySeconds;
            if (delay > 0d)
            {
                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken).ConfigureAwait(false);
            }

            var response = await _httpClient.GetAsync($"{_url}/{data.Name}", cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

            await _producer.ProduceAsync(new(data.Name, Convert.ToBase64String(bytes)), cancellationToken)
                           .ConfigureAwait(false);
        }
    }
}
