namespace Realmar.Jobbernetes.Framework.Options.RabbitMQ
{
    public class RabbitMQPubSubOptions
    {
        public string Exchange   { get; set; } = "jobbernetes-direct";
        public string Queue      { get; set; } = "jobbernetes-dev";
        public string BindingKey { get; set; } = "jobbernetes-default";
        public string RoutingKey { get; set; } = "jobbernetes-default";
    }
}
