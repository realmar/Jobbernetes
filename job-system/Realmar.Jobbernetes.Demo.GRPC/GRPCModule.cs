using Autofac;
using Grpc.Net.Client;
using Realmar.Jobbernetes.Demo.GRPC.ExternalService;

namespace Realmar.Jobbernetes.Demo.GRPC
{
    public class GRPCModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // TODO: actually pass that correctly
            builder.Register(context => GrpcChannel.ForAddress("https://localhost:5001"));

            builder.RegisterType<ImageService.ImageServiceClient>();
            builder.RegisterType<ExternalDataService.ExternalDataServiceClient>();
            builder.RegisterType<ExternalImageService.ExternalImageServiceClient>();
            builder.RegisterType<ExternalSpaceshipVendorService.ExternalSpaceshipVendorServiceClient>();
        }
    }
}
