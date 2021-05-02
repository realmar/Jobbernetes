using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Realmar.Jobbernetes.Demo.GRPC;
using Realmar.Jobbernetes.Utilities.Serialization.Kafka;

namespace Realmar.Jobbernetes.Demo.Ingress.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        [HttpPut]
        public async Task Put(string name)
        {
            var config = new ProducerConfig { BootstrapServers = "172.25.0.2:9094" };


            // If serializers are not specified, default serializers from
            // `Confluent.Kafka.Serializers` will be automatically used where
            // available. Note: by default strings are encoded as UTF8.

            var builder = new ProducerBuilder<Null, ImageIngress>(config);
            builder.SetValueSerializer(new ProtobufSerializer<ImageIngress>());

            using (var p = builder.Build())
            {
                try
                {
                    var dr = await p.ProduceAsync(
                        "test2",
                        new Message<Null, ImageIngress> { Value = new ImageIngress { Name = name } });
                    Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
                }
                catch (ProduceException<Null, byte[]> e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }
        }
    }
}
