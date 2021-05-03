namespace Realmar.Jobbernetes.Framework.Options
{
    public class KafkaOptions
    {
        // GroupId          = "test-consumer-group",
        // BootstrapServers = "172.25.0.2:9094",
#nullable disable
        public string GroupId          { get; set; } = "test-consumer-group";
        public string BootStrapServers { get; set; } = "172.25.0.2:9094";
        public string Topic            { get; set; } = "test";
#nullable enable
    }
}
