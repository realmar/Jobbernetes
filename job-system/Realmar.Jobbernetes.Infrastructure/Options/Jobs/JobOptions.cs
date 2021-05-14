namespace Realmar.Jobbernetes.Framework.Options.Jobs
{
    public class JobOptions
    {
        public int BatchSize          { get; set; } = 20;
        public int MaxConcurrentJobs  { get; set; } = 20;
        public int MaxMessagesPerTask { get; set; } = 20;
    }
}
