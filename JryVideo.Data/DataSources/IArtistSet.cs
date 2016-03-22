using JryVideo.Model;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JryVideo.Data.DataSources
{
    public interface IArtistSet : IJasilyEntitySetProvider<Artist, string>
    {
        Task<IEnumerable<Artist>> FindAsync(Artist.QueryParameter parameter);
    }
}