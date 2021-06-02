namespace Realmar.Jobbernetes.Framework.Options.RabbitMQ
{
    public class RabbitMQConsumerOptions : IExchangeOptionsProvider
    {
        public string Queue      { get; set; } = "jobbernetes-dev";
        public string BindingKey { get; set; } = "jobbernetes-default";
        public string Exchange   { get; set; } = "jobbernetes-direct";
    }
}
