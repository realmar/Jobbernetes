using Autofac;
using Realmar.Jobbernetes.Framework.Facade;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;

namespace Realmar.Jobbernetes.Framework
{
    public abstract class AutofacModule<TData> : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<FacadeModule<TData>>();
            builder.RegisterModule<JobsModule>();
            builder.RegisterModule<MessagingModule>();
        }
    }
}
