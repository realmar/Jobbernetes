using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Prometheus;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Demo.Egress
{
    public class EgressService : BackgroundService
    {
        private readonly IHostApplicationLifetime _application;
        private readonly IMongoCollection<Image>  _collection;
        private readonly IQueueConsumer<Image>    _consumer;
        private readonly Counter                  _counter;
        private readonly ILogger<EgressService>   _logger;

        public EgressService(IHostApplicationLifetime application,
                             IQueueConsumer<Image>    consumer,
                             IMongoCollection<Image>  collection,
                             IMetricsNameFactory      nameFactory,
                             ILogger<EgressService>   logger)
        {
            _application = application;
            _consumer    = consumer;
            _collection  = collection;
            _logger      = logger;

            var name = nameFactory.Create("exported_total");
            _counter = Metrics.CreateCounter(name, "Number of processed images", Labels.Keys.Status);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _consumer.StartAsync(async (image, token) =>
            {
                try
                {
                    await _collection.InsertOneAsync(image, options: null, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception jobException)
                {
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
