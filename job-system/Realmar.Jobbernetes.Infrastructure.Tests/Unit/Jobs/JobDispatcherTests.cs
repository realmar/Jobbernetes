using System.Threading;
using Autofac.Extras.Moq;
using Moq;
using Realmar.Jobbernetes.Infrastructure.Jobs;
using Xunit;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Unit.Jobs
{
    public class JobDispatcherTests
    {
        [Fact]
        public void Dispatch()
        {
            // Arrange
            using var mock       = AutoMock.GetLoose();
            var       dispatcher = mock.Create<JobDispatcher<object>>();

            // Act
            var data = new object();
            dispatcher.Dispatch(data);

            // Assert
            var job = mock.Mock<IJob<object>>();
            job.Verify(x => x.ProcessAsync(data, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
