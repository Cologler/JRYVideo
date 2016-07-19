using System.Collections.Generic;
using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Model.Interfaces;

namespace JryVideo.Data.DataSources
{
    public interface IQueryableEntitySet<T, in TQueryBy> : IEntitySet<T>
        where T : class, IJasilyEntity<string>, IQueryBy<TQueryBy>
    {
        Task<IEnumerable<T>> QueryAsync(TQueryBy parameter, int skip, int take);
    }
}