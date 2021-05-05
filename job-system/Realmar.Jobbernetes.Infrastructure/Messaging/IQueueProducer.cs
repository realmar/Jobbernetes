using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public interface IQueueProducer<in TData>
    {
        Task Produce(TData data);
    }
}
