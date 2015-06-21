using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface ICounterDataSourceProvider : IDataSourceProvider<JryCounter>
    {
        Task<IEnumerable<JryCounter>> QueryAsync(JryCounterType type);

        Task<bool> RefMathAsync(JryCounterType type, string value, int count);
    }
}