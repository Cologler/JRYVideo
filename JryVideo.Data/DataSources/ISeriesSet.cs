using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface ISeriesSet : IJasilyEntitySetProvider<JrySeries, string>
    {
        Task<IEnumerable<JrySeries>> QueryAsync(SearchElement search, int skip, int take);

        Task<IEnumerable<JrySeries>> ListTrackingAsync();
    }
}