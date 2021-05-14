using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prometheus.Client;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Demo.Ingress.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IMetricFamily<ICounter, ValueTuple<string>> _counter;
        private readonly ILogger<ImagesController>                   _logger;
        private readonly IQueueProducer<ImageIngress>                _producer;

        public ImagesController(IQueueProducer<ImageIngress> producer,
                                IMetricsNameFactory          nameFactory,
                                ILogger<ImagesController>    logger,
                                IMetricFactory               metricFactory)
        {
            _producer = producer;
            _logger   = logger;

            var name = nameFactory.Create("images_inserted");
            _counter = metricFactory.CreateCounter(name, "Number of images inserted into the system.", Labels.Keys.Status);
        }

        [HttpPut]
        public async Task Put(string name, CancellationToken cancellationToken)
        {
            try
            {
                await _producer.ProduceAsync(new(name), cancellationToken).ConfigureAwait(false);
                _counter.WithSuccess().Inc();
            }
            catch (Exception e)
            {
                _counter.WithFail().Inc();
                _logger.LogError(e, $"Failed to insert image Name = {name}");
                throw;
            }
        }
    }
}
