namespace Realmar.Jobbernetes.Framework.Options
{
    public class KafkaOptions
    {
        // GroupId          = "test-consumer-group",
        // BootstrapServers = "172.25.0.2:9094",
#nullable disable
        public string GroupId          { get; set; } = "test-consumer-group-22";
        public string BootStrapServers { get; set; } = "172.25.0.3:9094";
        public string Topic            { get; set; } = "test8";
        public int    PollTimeout      { get; set; } = 2;
#nullable enable
    }
}
