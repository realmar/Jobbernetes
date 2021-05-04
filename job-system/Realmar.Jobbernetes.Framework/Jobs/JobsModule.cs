using Autofac;

namespace Realmar.Jobbernetes.Framework.Jobs
{
    public class JobsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(JobDispatcher<>)).AsImplementedInterfaces();
        }
    }
}
