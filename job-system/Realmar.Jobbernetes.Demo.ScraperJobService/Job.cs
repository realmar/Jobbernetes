using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;

namespace Realmar.Jobbernetes.Demo.ScraperJobService
{
    internal class Job : IJob<ImageIngress>
    {
        private readonly HttpClient            _httpClient;
        private readonly IQueueProducer<Image> _producer;
        private readonly string                _url;

        public Job(HttpClient httpClient, IQueueProducer<Image> producer, IOptions<ExternalServiceOptions> options)
        {
            _httpClient = httpClient;
            _producer   = producer;
            _url        = options.Value.Url;
        }

        public async Task Process(ImageIngress data, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"{_url}/{data.Name}", cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

            await _producer.ProduceAsync(new(data.Name, Convert.ToBase64String(bytes)), cancellationToken)
                           .ConfigureAwait(false);
        }
    }
}
