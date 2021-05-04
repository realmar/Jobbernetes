using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Realmar.Jobbernetes.Demo.GRPC;
using Realmar.Jobbernetes.Framework.Messaging;

namespace Realmar.Jobbernetes.Demo.Ingress.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IQueueProducer<ImageIngress> _producer;

        public ImagesController(IQueueProducer<ImageIngress> producer) => _producer = producer;

        [HttpPut]
        public Task Put(string name)
        {
            return _producer.Produce(new() { Name = name });
        }
    }
}
