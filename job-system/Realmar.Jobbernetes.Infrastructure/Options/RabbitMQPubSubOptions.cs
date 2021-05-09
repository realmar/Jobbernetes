namespace Realmar.Jobbernetes.Framework.Options
{
    public class RabbitMQPubSubOptions
    {
        public string Exchange   { get; set; } = "jobbernetes-direct";
        public string Queue      { get; set; } = "jobbernetes-dev";
        public string RoutingKey { get; set; } = "jobbernetes-default";
    }
}
