using Jasily.Data;
using JryVideo.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JryVideo.Data.DataSources
{
    public interface IFlagableSet<T> : IJasilyEntitySetProvider<T, string>
        where T : class, IJasilyEntity<string>
    {
        Task<IEnumerable<T>> QueryAsync(JryFlagType type, string flag);
    }
}