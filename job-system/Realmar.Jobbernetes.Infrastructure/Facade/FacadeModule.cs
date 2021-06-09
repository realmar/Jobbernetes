using Autofac;

namespace Realmar.Jobbernetes.Infrastructure.Facade
{
    public class FacadeModule<TData> : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Jobbernetes<TData>>().AsImplementedInterfaces();
        }
    }
}
