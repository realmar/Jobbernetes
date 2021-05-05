using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Framework.Messaging;

namespace Realmar.Jobbernetes.Demo.Egress
{
    public class EgressService : BackgroundService
    {
        private readonly IHostApplicationLifetime _application;
        private readonly IMongoCollection<Image>  _collection;
        private readonly IQueueConsumer<Image>    _consumer;

        public EgressService(IHostApplicationLifetime application,
                             IQueueConsumer<Image>    consumer,
                             IMongoCollection<Image>  collection)
        {
            _application = application;
            _consumer    = consumer;
            _collection  = collection;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await foreach (var image in _consumer.Consume(default).ConfigureAwait(false))
            {
                await _collection.InsertOneAsync(image, options: null, cancellationToken).ConfigureAwait(false);
            }

            _application.StopApplication();
        }
    }
}
