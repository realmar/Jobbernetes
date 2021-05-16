using Microsoft.AspNetCore.Mvc;
using Prometheus.Client;
using Realmar.Jobbernetes.Infrastructure.Metrics;
using SkiaSharp;

namespace Realmar.Jobbernetes.Demo.ExternalImageService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ICounter   _counter;
        private readonly SKTypeface _typeface;

        public ImagesController(SKTypeface typeface, IMetricsNameFactory nameFactory, IMetricFactory metricFactory)
        {
            _typeface = typeface;
            _counter = metricFactory.CreateCounter(nameFactory.Create("bytes_total"),
                                                   "Number of bytes of all generated images");
        }

        [HttpGet("{name}")]
        public IActionResult Get(string name)
        {
            using SKPaint textPaint = new()
            {
                TextSize = 86,
                Color    = SKColor.Parse("#FFFFFF"),
                Typeface = _typeface
            };

            var bounds = new SKRect();
            textPaint.MeasureText(name, ref bounds);

            var helloBitmap = new SKBitmap((int) bounds.Right, (int) bounds.Height);

            using var bitmapCanvas = new SKCanvas(helloBitmap);
            bitmapCanvas.Clear();
            bitmapCanvas.DrawText(name, 0, -bounds.Top, textPaint);

            using var data         = helloBitmap.Encode(SKEncodedImageFormat.Jpeg, 28);
            var       fileContents = data.ToArray();

            _counter.Inc(fileContents.Length);

            return File(fileContents, "image/jpeg");
        }
    }
}
