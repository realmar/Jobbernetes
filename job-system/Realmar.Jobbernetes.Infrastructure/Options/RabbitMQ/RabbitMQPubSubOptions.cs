namespace Realmar.Jobbernetes.Infrastructure.Options.RabbitMQ
{
    public abstract class RabbitMQPubSubOptions
    {
        public string Exchange   { get; set; } = "jobbernetes-direct";
        public string Queue      { get; set; } = "jobbernetes-dev";
        public string BindingKey { get; set; } = "jobbernetes-default";
        public string RoutingKey { get; set; } = "jobbernetes-default";
    }
}
