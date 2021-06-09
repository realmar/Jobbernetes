namespace Realmar.Jobbernetes.Demo.Infrastructure.MongoDB.Options
{
    public class MongoOptions
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string Database         { get; set; } = "jobbernetes";
        public string Collection       { get; set; } = "images";
    }
}
