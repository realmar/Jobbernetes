using System;
using System.Threading;
using Confluent.Kafka;
using MongoDB.Driver;
using Realmar.Jobbernetes.Demo.GRPC;
using Realmar.Jobbernetes.Demo.Utilities.Serialization.MongoDB;
using Realmar.Jobbernetes.Utilities.Serialization.Kafka;

namespace Realmar.Jobbernetes.Demo.Egress
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            BsonClassMapper.MapProtobufModels();
            var client   = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("jobbernetes");
            var collection =
                database!.GetCollection<Image>("images", new MongoCollectionSettings { AssignIdOnInsert = true });

            // collection.InsertOne(new SampleDocument(1, "A"));
            // collection.InsertOne(new SampleDocument(2, "B"));


            // collection.InsertOne(new Image("Test", Encoding.UTF8.GetBytes("this is the image")));


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

            var builder = new ConsumerBuilder<Ignore, Image>(conf);
            builder.SetValueDeserializer(new ProtobufDeserializer<Image>());

            using (var c = builder.Build())
            {
                c.Subscribe("test");

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
                            collection.InsertOne(image);
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
    }
}
