using System;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework
{
    public class Jobbernetes : IJobbernetes
    {
        public Task<TInput> GetDataAsync<TInput>(in DataDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public Task StoreDataAsync<TOutput>(TOutput data, in DataDescriptor descriptor)
        {
            throw new NotImplementedException();
        }
    }
}
