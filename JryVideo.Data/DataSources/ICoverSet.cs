using JryVideo.Model;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JryVideo.Data.DataSources
{
    public interface ICoverSet : IJasilyEntitySetProvider<JryCover, string>
    {
        Task<IEnumerable<JryCover>> FindAsync(JryCover.QueryParameter parameter);
    }
}