using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Realmar.Jobbernetes.Demo.GRPC;
using Realmar.Jobbernetes.Demo.Utilities.Serialization.MongoDB;
using Realmar.Jobbernetes.Extensions.Serialization.Kafka;
using Realmar.Jobbernetes.Framework.Messaging;

namespace Realmar.Jobbernetes.Demo.Egress
{
    public class HackService : BackgroundService
    {
        private readonly IHostApplicationLifetime _application;
        private readonly IQueueConsumer<Image>    _consumer;

        public HackService(IQueueConsumer<Image> consumer, IHostApplicationLifetime application)
        {
            _consumer    = consumer;
            _application = application;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            BsonClassMapper.MapProtobufModels();
            var client   = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("jobbernetes");
            var collection =
                database!.GetCollection<Image>("images", new() { AssignIdOnInsert = true });


            await foreach (var image in _consumer.Consume(default).ConfigureAwait(false))
            {
                await collection.InsertOneAsync(image).ConfigureAwait(false);
            }

            _application.StopApplication();
        }
    }

    // TODO: refactor into base
    public static class HackHost
    {
        public static Task StartAsync() =>
            Host.CreateDefaultBuilder(Environment.GetCommandLineArgs())
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging(ConfigureLogging)
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices)
                .ConfigureContainer<ContainerBuilder>(ConfigureDelegate)
                .RunConsoleAsync();

        private static void ConfigureDelegate(HostBuilderContext context, ContainerBuilder builder)
        {
            builder.RegisterModule<MessagingModule>();
            builder.UseKafkaProtobuf<Image>();
        }

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.AddConsole();
        }

        private static void ConfigureAppConfiguration(IConfigurationBuilder builder)
        {
            builder.AddEnvironmentVariables();
            builder.AddIniFile("appsettings.ini", optional: true, reloadOnChange: true);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<HackService>();
        }

        private static void ConfigureContainer<TModule>(ContainerBuilder builder)
            where TModule : class, IModule, new()
        {
            builder.RegisterModule<TModule>();
        }
    }

    internal static class Program
    {
        private static Task Main(string[] args)
        {
            return HackHost.StartAsync();


            /*BsonClassMapper.MapProtobufModels();
            var client   = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("jobbernetes");
            var collection =
                database!.GetCollection<Image>("images", new() { AssignIdOnInsert = true });

            // collection.InsertOne(new SampleDocument(1, "A"));
            // collection.InsertOne(new SampleDocument(2, "B"));


            // collection.InsertOne(new Image("Test", Encoding.UTF8.GetBytes("this is the image")));


            var conf = new ConsumerConfig
            {
                GroupId          = "test-consumer-group",
                BootstrapServers = "172.25.0.3:9094",
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
                    {
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
                }
                catch (OperationCanceledException)
                {
                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    c.Close();
                }
            }*/
        }
    }
}
