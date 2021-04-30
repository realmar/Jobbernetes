using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework
{
    public interface IJobbernetes
    {
        Task<TInput> GetDataAsync<TInput>(in DataDescriptor descriptor);

        Task StoreDataAsync<TOutput>(TOutput data, in DataDescriptor descriptor);
    }
}
