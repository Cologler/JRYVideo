using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface IFlagDataSourceProvider : IDataSourceProvider<JryFlag>
    {
        Task<IEnumerable<JryFlag>> QueryAsync(JryFlagType type);

        Task<bool> RefMathAsync(JryFlagType type, string value, int count);
    }
}