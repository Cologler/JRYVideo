using System.Collections.Generic;
using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

namespace JryVideo.Data.DataSources
{
    public interface IQueryableEntitySet<T, in TQueryBy> : IEntitySet<T>
        where T : class, IJasilyEntity<string>, IQueryBy<TQueryBy>
    {
        Task<IEnumerable<Series>> QueryAsync(TQueryBy queryParameter, int skip, int take);
    }
}