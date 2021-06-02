namespace Realmar.Jobbernetes.Framework.Options.RabbitMQ
{
    public class RabbitMQProducerOptions : IExchangeOptionsProvider
    {
        public string RoutingKey { get; set; } = "jobbernetes-default";
        public string Exchange   { get; set; } = "jobbernetes-direct";
    }
}
