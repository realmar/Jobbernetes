using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MongoDB.Driver;
using Realmar.Jobbernetes.Demo.GRPC;

namespace Realmar.Jobbernetes.Demo.DataViewer.Server.Services
{
    public class ImagesService : ImageService.ImageServiceBase
    {
        public override Task GetImages(Empty             request, IServerStreamWriter<Image> responseStream,
                                       ServerCallContext context)
        {
            var client   = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("jobbernetes");
            var collection =
                database!.GetCollection<Image>("images", new MongoCollectionSettings { AssignIdOnInsert = true });

            // collection.InsertOne(new SampleDocument(1, "A"));
            // collection.InsertOne(new SampleDocument(2, "B"));


            // collection.InsertOne(new Image("Test", Encoding.UTF8.GetBytes("this is the image")));


            var images = collection.AsQueryable();
            return images.ForEachAsync(responseStream.WriteAsync);
        }
    }
}
