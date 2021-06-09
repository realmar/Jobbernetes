using Autofac.Extras.Moq;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Infrastructure.Metrics;
using Realmar.Jobbernetes.Infrastructure.Options.Metrics;
using Xunit;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Unit.Metrics
{
    public class MetricsNameFactoryTests
    {
        [Fact]
        public void Create()
        {
            // Arrange
            using var    mock       = AutoMock.GetLoose();
            const string basePrefix = "JobbernetesTests";
            const string name       = "UUT";

            mock.Mock<IOptions<MetricsOptions>>()
                .Setup(options => options.Value)
                .Returns(new MetricsOptions { BasePrefix = basePrefix });
            var factory = mock.Create<MetricsNameFactory>();

            // Act
            var actual = factory.Create(name);

            // Assert
            Assert.Equal($"{basePrefix}_{name}", actual);
        }
    }
}
