using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface ISeriesDataSourceProvider : IDataSourceProvider<JrySeries>
    {
        Task<IEnumerable<JrySeries>> QueryAsync(SearchElement search, int skip, int take);

        Task<IEnumerable<JrySeries>> ListTrackingAsync();
    }
}