using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface ISeriesSet : IQueryableEntitySet<Series, Series.QueryParameter>
    {
        Task<IEnumerable<Series>> ListTrackingAsync();
    }
}