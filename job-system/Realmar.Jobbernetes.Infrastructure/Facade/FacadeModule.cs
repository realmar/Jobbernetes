using Autofac;

namespace Realmar.Jobbernetes.Framework.Facade
{
    public class FacadeModule<TData> : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Jobbernetes<TData>>().AsImplementedInterfaces();
            builder.RegisterType<Watcher>().AsSelf();
        }
    }
}
