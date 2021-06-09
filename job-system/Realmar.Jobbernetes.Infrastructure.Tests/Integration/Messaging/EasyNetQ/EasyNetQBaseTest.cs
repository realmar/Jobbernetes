using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Options;
using Moq;
using Realmar.Jobbernetes.Infrastructure.Options.Jobs;
using Realmar.Jobbernetes.Infrastructure.Options.RabbitMQ;
using ISerializer = Realmar.Jobbernetes.Infrastructure.Messaging.Serialization.ISerializer;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Integration.Messaging.EasyNetQ
{
#nullable disable
    public class EasyNetQBaseTest : IDisposable
    {
        private   AutoMock                           _mock;
        protected Mock<ISerializer>                  SerializerMock      { get; private set; }
        protected Mock<IBus>                         BusMock             { get; private set; }
        protected Mock<IAdvancedBus>                 AdvancedBusMock     { get; private set; }
        protected Mock<IPullingConsumer<PullResult>> PullingConsumerMock { get; private set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _mock?.Dispose();
        }

        protected TSystem Arrange<TSystem>()
        {
            return Arrange<TSystem>(new(new()
            {
                BatchSize              = 10,
                MaxDegreeOfParallelism = 2
            }, int.MaxValue));
        }

        protected TSystem Arrange<TSystem>(TestOptions options)
        {
            // Arrange
            _mock = AutoMock.GetLoose();

            SerializerMock      = _mock!.Mock<ISerializer>();
            BusMock             = _mock.Mock<IBus>();
            AdvancedBusMock     = _mock.Mock<IAdvancedBus>();
            PullingConsumerMock = _mock.Mock<IPullingConsumer<PullResult>>();

            ArrangeSerializer();
            ArrangeOptions(options);
            ArrangeAdvancedBus();
            ArrangePullingConsumer(options);

            return _mock.Create<TSystem>();
        }

        private void ArrangeAdvancedBus()
        {
            BusMock.SetupGet(bus => bus.Advanced).Returns(AdvancedBusMock.Object);
        }

        private void ArrangePullingConsumer(TestOptions options)
        {
            AdvancedBusMock.Setup(bus => bus.CreatePullingConsumer(It.IsAny<IQueue>(), It.IsAny<bool>()))
                           .Returns(PullingConsumerMock.Object);

            var messageCounter = 0;
            PullingConsumerMock.Setup(pullingConsumer => pullingConsumer.PullAsync(It.IsAny<CancellationToken>()))
                               .Returns(() =>
                                {
                                    if (++messageCounter > options.MessageCount)
                                    {
                                        return Task.FromResult(PullResult.NotAvailable);
                                    }

                                    return Task.FromResult(PullResult.Available(
                                                               1, new("test_consumer", 0, false, "test", "test", "test"),
                                                               new(), Encoding.UTF8.GetBytes("Hello World")));
                                });
        }

        private void ArrangeOptions(TestOptions options)
        {
            void SetupOptions<TOptions>(Func<TOptions>? options = null)
                where TOptions : class, new() =>
                _mock.Mock<IOptions<TOptions>>()
                     .SetupGet(options => options.Value)
                     .Returns(() => options?.Invoke() ?? new TOptions());

            SetupOptions(() => options.JobOptions);
            SetupOptions<RabbitMQConsumerOptions>();
            SetupOptions<RabbitMQProducerOptions>();
        }

        protected void ArrangeFaultySerializer()
        {
            ArrangeSerializer((_, __) => throw new(),
                              _ => throw new(),
                              (_, __) => throw new(),
                              _ => throw new());
        }

        private void ArrangeSerializer()
        {
            ArrangeSerializer((_, data) => Encoding.UTF8.GetBytes((string) data),
                              data => Encoding.UTF8.GetBytes(data),
                              (_, bytes) => Encoding.UTF8.GetString(bytes.Span),
                              bytes => Encoding.UTF8.GetString(bytes.Span));
        }


        private void ArrangeSerializer(Func<Type, object, ReadOnlyMemory<byte>> serialize,
                                       Func<string, ReadOnlyMemory<byte>>       serializeGeneric,
                                       Func<Type, ReadOnlyMemory<byte>, object> deserialize,
                                       Func<ReadOnlyMemory<byte>, string>       deserializeGeneric)
        {
            SerializerMock.Reset();

            SerializerMock!.Setup(serializer => serializer.Serialize(It.IsAny<Type>(), It.IsAny<object>()))
                           .Returns(serialize);

            SerializerMock.Setup(serializer => serializer.Serialize(It.IsAny<string>()))
                          .Returns(serializeGeneric);

            SerializerMock.Setup(serializer => serializer.Deserialize(It.IsAny<Type>(), It.IsAny<ReadOnlyMemory<byte>>()))
                          .Returns(deserialize);

            SerializerMock.Setup(serializer => serializer.Deserialize<string>(It.IsAny<ReadOnlyMemory<byte>>()))
                          .Returns(deserializeGeneric);
        }

        protected record TestOptions(JobOptions JobOptions, int MessageCount);
    }
#nullable restore
}
