using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Prometheus;
using Realmar.Jobbernetes.Infrastructure.Metrics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Realmar.Jobbernetes.Demo.ExternalImageService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly Counter _counter;
        private readonly Font    _font;

        public ImagesController(Font font, IMetricsNameFactory nameFactory)
        {
            _font    = font;
            _counter = Metrics.CreateCounter(nameFactory.Create("bytes_total"), "Number of bytes of all generated images");
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            const int width    = 512;
            const int height   = 512;
            const int fontSize = 28;

            await using var stream = new MemoryStream();
            using var       image  = new Image<Rgba32>(width, height);

            image.Mutate(context => context.BackgroundColor(Color.Black));
            image.Mutate(context => context.DrawText(name, _font, Color.White,
                                                     new(fontSize * 2f, height / 2f - fontSize / 2f)));

            await image.SaveAsync(stream, new JpegEncoder()).ConfigureAwait(false);

            stream.ConfigureAwait(false);
            var fileContents = stream.ToArray();

            _counter.Inc(fileContents.Length);

            return File(fileContents, "image/jpeg");
        }
    }
}
