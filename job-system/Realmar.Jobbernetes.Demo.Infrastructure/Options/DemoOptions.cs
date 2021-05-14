namespace Realmar.Jobbernetes.Demo.Infrastructure.Options
{
    public class DemoOptions
    {
        public Range  ProcessingDelayMilliseconds { get; set; } = new(0, 0);
        public double FailureProbability          { get; set; }

        public record Range(int Min, int Max);
    }
}
