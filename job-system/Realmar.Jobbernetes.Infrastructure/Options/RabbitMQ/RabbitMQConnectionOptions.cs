namespace Realmar.Jobbernetes.Infrastructure.Options.RabbitMQ
{
    public class RabbitMQConnectionOptions
    {
        public string Hostname { get; set; } = "localhost";
        public int    Port     { get; set; } = 5672;
        public string Username { get; set; } = "admin";
        public string Password { get; set; } = "admin";
    }
}
