using Autofac;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Realmar.Jobbernetes.Demo.Infrastructure.MongoDB.Options;

namespace Realmar.Jobbernetes.Demo.Infrastructure.MongoDB
{
    public class MongoDBModule<TData> : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var options  = context.Resolve<IOptions<MongoOptions>>().Value;
                var client   = new MongoClient(options.ConnectionString);
                var database = client.GetDatabase(options.Database);
                return database.GetCollection<TData>(
                    options.Collection, new() { AssignIdOnInsert = true });
            }).As<IMongoCollection<TData>>();
        }
    }
}
