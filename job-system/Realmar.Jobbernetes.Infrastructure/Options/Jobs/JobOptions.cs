namespace Realmar.Jobbernetes.Infrastructure.Options.Jobs
{
    public class JobOptions
    {
        public int BatchSize              { get; set; } = 20;
        public int MaxDegreeOfParallelism { get; set; } = 20;
    }
}
