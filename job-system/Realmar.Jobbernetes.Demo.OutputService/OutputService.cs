using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Prometheus.Client;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Infrastructure.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Demo.OutputService
{
    public class OutputService : BackgroundService
    {
        private readonly IHostApplicationLifetime                    _application;
        private readonly IMongoCollection<ImageOutput>               _collection;
        private readonly IQueueConsumer<ImageOutput>                 _consumer;
        private readonly IMetricFamily<ICounter, ValueTuple<string>> _counter;
        private readonly ILogger<OutputService>                      _logger;

        public OutputService(IHostApplicationLifetime      application,
                             IQueueConsumer<ImageOutput>   consumer,
                             IMongoCollection<ImageOutput> collection,
                             IMetricsNameFactory           nameFactory,
                             ILogger<OutputService>        logger,
                             IMetricFactory                metricFactory)
        {
            _application = application;
            _consumer    = consumer;
            _collection  = collection;
            _logger      = logger;

            var name = nameFactory.Create("output_images_total");
            _counter = metricFactory.CreateCounter(name, "Number of processed images", Labels.Keys.Status);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _consumer.StartAsync(async (image, token) =>
            {
                try
                {
                    await _collection.InsertOneAsync(image, options: null, token).ConfigureAwait(false);
                    _counter.WithSuccess().Inc();
                }
                catch (Exception jobException)
                {
                    _counter.WithFail().Inc();
                    _logger.LogError(jobException, "Failed to save image to DB");
                    throw;
                }
            }, cancellationToken).ConfigureAwait(false);

            cancellationToken.WaitHandle.WaitOne();

            await _consumer.StopAsync(default).ConfigureAwait(false);

            _application.StopApplication();
        }
    }
}
