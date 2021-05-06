namespace Realmar.Jobbernetes.Framework.Options
{
    public class KafkaConsumerOptions
    {
        public string GroupId     { get; set; } = "jobbernetes-dev-group";
        public string Topic       { get; set; } = "jobbernetes-dev";
        public int    PollTimeout { get; set; } = 2;
    }
}
