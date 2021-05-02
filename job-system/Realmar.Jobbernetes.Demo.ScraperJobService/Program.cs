using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Grpc.Net.Client;
using Realmar.Jobbernetes.Demo.GRPC;
using Realmar.Jobbernetes.Demo.GRPC.ExternalService;
using Realmar.Jobbernetes.Utilities.Serialization.Kafka;
using ImageService = Realmar.Jobbernetes.Demo.GRPC.ExternalService.ImageService;

namespace Realmar.Jobbernetes.Demo.ScraperJobService
{
    internal static class Program
    {
        private static async Task Main()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);

            var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions { HttpClient = httpClient });
            var client  = new ImageService.ImageServiceClient(channel);

            // var response = await client.GetImageAsync(new ImageRequest { Name = "Hello World" });

            // File.WriteAllBytes("demo.jpg", response.Data.ToByteArray());

            var conf = new ConsumerConfig
            {
                GroupId          = "test-consumer-group",
                BootstrapServers = "172.25.0.2:9094",
                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this example, consumption will only start from the
                // earliest message in the topic 'my-topic' the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var config = new ProducerConfig { BootstrapServers = "172.25.0.2:9094" };


            // If serializers are not specified, default serializers from
            // `Confluent.Kafka.Serializers` will be automatically used where
            // available. Note: by default strings are encoded as UTF8.

            var builder = new ProducerBuilder<Null, Image>(config);
            builder.SetValueSerializer(new ProtobufSerializer<Image>());

            using (var p = builder.Build())
            {
                try
                {
                    var builder2 = new ConsumerBuilder<Ignore, ImageIngress>(conf);
                    builder2.SetValueDeserializer(new ProtobufDeserializer<ImageIngress>());

                    using (var c = builder2.Build())
                    {
                        c.Subscribe("test2");

                        var cts = new CancellationTokenSource();

                        try
                        {
                            while (true)
                                try
                                {
                                    var cr = c.Consume(cts.Token);
                                    Console.WriteLine($"Consumed message at {cr.TopicPartitionOffset}");

                                    var image = cr.Message.Value;

                                    // collection.InsertOne(new SampleDocument(cr.Value, cr.TopicPartitionOffset.ToString()));
                                    // collection.InsertOne(image);

                                    var response = await client.GetImageAsync(new ImageRequest { Name = image.Name });

                                    var dr = await p.ProduceAsync(
                                        "test",
                                        new Message<Null, Image>
                                        {
                                            Value = new Image { Name = image.Name, Data = response.Data.ToBase64() }
                                        });
                                    Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
                                }
                                catch (ConsumeException e)
                                {
                                    Console.WriteLine($"Error occured: {e.Error.Reason}");
                                }
                        }
                        catch (OperationCanceledException)
                        {
                            // Ensure the consumer leaves the group cleanly and final offsets are committed.
                            c.Close();
                        }
                    }
                }
                catch (ProduceException<Null, byte[]> e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }
        }
    }
}
