using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface ICounterDataSourceProvider : IDataSourceProvider<JryCounter>
    {
        Task<bool> RefAddAsync(JryCounterType type, string value, int count);

        Task<bool> RefAddOneAsync(JryCounterType type, string value);

        Task<bool> RefSubAsync(JryCounterType type, string value, int count);

        Task<bool> RefSubOneAsync(JryCounterType type, string value);
    }
}