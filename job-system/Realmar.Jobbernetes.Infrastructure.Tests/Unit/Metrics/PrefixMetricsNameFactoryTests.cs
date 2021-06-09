using Autofac;
using Autofac.Extras.Moq;
using Moq;
using Realmar.Jobbernetes.Infrastructure.Metrics;
using Xunit;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Unit.Metrics
{
    public class PrefixMetricsNameFactoryTests
    {
        [Fact]
        public void Create()
        {
            // Arrange
            using var mock = AutoMock.GetLoose();

            var prefix   = "Prefix";
            var name     = "Metric";
            var baseName = "Base";

            mock.Mock<IMetricsNameFactory>()
                .Setup(nameFactory => nameFactory.Create(It.IsAny<string>()))
                .Returns<string>(name => $"{baseName}_{name}");
            var factory = mock.Create<PrefixMetricsNameFactory>(new TypedParameter(typeof(string), prefix));

            // Act
            var actual = factory.Create(name);

            // Assert
            Assert.Equal($"{baseName}_{prefix}_{name}", actual);
        }
    }
}
