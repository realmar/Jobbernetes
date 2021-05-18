namespace Realmar.Jobbernetes.Demo.ImageScrapeJob.Options
{
    internal class DemoOptions
    {
        public string TextPrefix                  { get; set; } = "";
        public string TextPostfix                 { get; set; } = "";
        public Range  ProcessingDelayMilliseconds { get; set; } = new(0, 0);
        public double FailureProbability          { get; set; }

        public record Range(int Min, int Max);
    }
}
