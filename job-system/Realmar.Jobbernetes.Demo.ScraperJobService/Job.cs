using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Realmar.Jobbernetes.Demo.GRPC;
using Realmar.Jobbernetes.Demo.GRPC.ExternalService;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;

namespace Realmar.Jobbernetes.Demo.ScraperJobService
{
    public class Job : IJob<ImageIngress>
    {
        private readonly ExternalImageService.ExternalImageServiceClient _client;
        private readonly IQueueProducer<Image>                           _producer;

        public Job( /*ExternalImageService.ExternalImageServiceClient client, */ IQueueProducer<Image> producer)
        {
            var httpClientHandler = new HttpClientHandler();
            // Return `true` to allow certificates that are untrusted/invalid
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);

            _client = new(GrpcChannel.ForAddress("https://localhost:5001",
                                                 new() { HttpClient = httpClient })); // client;
            _producer = producer;
        }

        public async Task Process(ImageIngress data, CancellationToken cancellationToken)
        {
            var response = await _client.GetImageAsync(new() { Name = data.Name });
            await _producer.Produce(new() { Name = data.Name, Data = response.Data.ToBase64() })
                           .ConfigureAwait(false);
        }
    }
}
