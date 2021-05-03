namespace Realmar.Jobbernetes.Framework.Jobs
{
    public interface IJobFactory<in TData>
    {
        IJob<TData> Create();
    }
}
