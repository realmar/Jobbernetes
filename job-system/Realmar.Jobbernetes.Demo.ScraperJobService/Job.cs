using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;

namespace Realmar.Jobbernetes.Demo.ScraperJobService
{
    internal class Job : IJob<ImageIngress>
    {
        private readonly HttpClient            _httpClient;
        private readonly IQueueProducer<Image> _producer;

        public Job(HttpClient httpClient, IQueueProducer<Image> producer)
        {
            _httpClient = httpClient;
            _producer   = producer;
        }

        public async Task Process(ImageIngress data, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"http://localhost:5000/images/{data.Name}").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

            await _producer.Produce(new(data.Name, Convert.ToBase64String(bytes)))
                           .ConfigureAwait(false);
        }
    }
}
