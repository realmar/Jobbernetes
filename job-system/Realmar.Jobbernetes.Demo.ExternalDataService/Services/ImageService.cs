using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using Google.Protobuf;
using Grpc.Core;
using Realmar.Jobbernetes.Demo.GRPC.ExternalService;

namespace Realmar.Jobbernetes.Demo.ExternalDataService.Services
{
    public class ImageService : ExternalImageService.ExternalImageServiceBase
    {
        public override Task<ImageReply> GetImage(ImageRequest request, ServerCallContext context)
        {
            var name = request.Name;

            var http = new HttpClient();

            var images = new Faker<ImageReply>()
               .RuleFor(reply => reply.Data, faker =>
                {
                    var url   = faker.Image.PicsumUrl();
                    var bytes = http.GetByteArrayAsync(url).GetAwaiter().GetResult();

                    return ByteString.CopyFrom(bytes);
                });

            return Task.FromResult(images.Generate());
        }
    }
}
