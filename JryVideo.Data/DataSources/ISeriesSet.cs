using System.Collections.Generic;
using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface ISeriesSet : IJasilyEntitySetProvider<Series, string>
    {
        Task<IEnumerable<Series>> QueryAsync(Series.QueryParameter search, int skip, int take);

        Task<IEnumerable<Series>> ListTrackingAsync();
    }
}