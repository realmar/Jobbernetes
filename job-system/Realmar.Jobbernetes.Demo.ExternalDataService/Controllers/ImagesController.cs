using Microsoft.AspNetCore.Mvc;
using NetVips;
using Prometheus.Client;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Demo.ExternalImageService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ICounter _counter;

        public ImagesController(IMetricsNameFactory nameFactory, IMetricFactory metricFactory)
        {
            _counter = metricFactory.CreateCounter(nameFactory.Create("bytes_total"),
                                                   "Number of bytes of all generated images");
        }

        [HttpGet("{name}")]
        public IActionResult Get(string name)
        {
            const int width  = 256;
            const int height = 256;

            if (ModuleInitializer.VipsInitialized)
            {
                using var textImage    = Image.Text(name, width: width, height: height, dpi: 600);
                var       fileContents = textImage.JpegsaveBuffer();

                _counter.Inc(fileContents.Length);

                return File(fileContents, "image/jpeg");
            }

            throw ModuleInitializer.Exception;
        }
    }
}
