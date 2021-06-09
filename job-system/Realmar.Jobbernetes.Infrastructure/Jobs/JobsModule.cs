using Autofac;

namespace Realmar.Jobbernetes.Infrastructure.Jobs
{
    public class JobsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(JobDispatcher<>)).AsImplementedInterfaces();
        }
    }
}
