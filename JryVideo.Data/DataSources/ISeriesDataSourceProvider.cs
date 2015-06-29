using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface ISeriesDataSourceProvider : IDataSourceProvider<JrySeries>
    {
        Task<IEnumerable<JrySeries>> QueryByNameAsync(string searchText, int skip, int take);
    }
}