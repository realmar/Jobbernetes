using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prometheus.Client;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Infrastructure.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Demo.InputService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IMetricFamily<ICounter, ValueTuple<string>> _counter;
        private readonly ILogger<ImagesController>                   _logger;
        private readonly IQueueProducer<ImageInput>                  _producer;

        public ImagesController(IQueueProducer<ImageInput> producer,
                                IMetricsNameFactory        nameFactory,
                                ILogger<ImagesController>  logger,
                                IMetricFactory             metricFactory)
        {
            _producer = producer;
            _logger   = logger;

            var name = nameFactory.Create("input_text_total");
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
