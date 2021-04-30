using Autofac;

namespace Realmar.Jobbernetes.Framework
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder!.RegisterType<Jobbernetes>().AsImplementedInterfaces();
        }
    }
}
