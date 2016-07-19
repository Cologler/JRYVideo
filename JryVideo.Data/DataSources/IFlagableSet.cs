using System.Collections.Generic;
using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface IFlagableSet<T> : IEntitySet<T>
        where T : class, IJasilyEntity<string>
    {
        Task<IEnumerable<T>> QueryAsync(JryFlagType type, string flag);
    }
}