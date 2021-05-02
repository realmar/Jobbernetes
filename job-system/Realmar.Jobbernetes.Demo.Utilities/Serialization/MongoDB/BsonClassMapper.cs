using MongoDB.Bson.Serialization;
using Realmar.Jobbernetes.Demo.GRPC;

namespace Realmar.Jobbernetes.Demo.Utilities.Serialization.MongoDB
{
    public static class BsonClassMapper
    {
        private static readonly object Lock = new();
        private static          bool   _mapped;

        public static void MapProtobufModels()
        {
            lock (Lock)
            {
                if (_mapped) return;

                BsonClassMap.RegisterClassMap<Image>(map =>
                {
                    map.MapProperty(image => image.Name);
                    map.MapProperty(image => image.Data);
                });

                _mapped = true;
            }
        }
    }
}
