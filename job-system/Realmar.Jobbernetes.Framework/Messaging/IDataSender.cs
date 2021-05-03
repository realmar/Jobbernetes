using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public interface IDataSender<TData>
    {
        Task Send(TData data);
    }
}
