using JryVideo.Model;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JryVideo.Data.DataSources
{
    public interface IArtistSet : IJasilyEntitySetProvider<JryArtist, string>
    {
        Task<IEnumerable<JryArtist>> FindAsync(JryArtist.QueryParameter parameter);
    }
}