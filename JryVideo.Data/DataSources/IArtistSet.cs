using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface IArtistSet : IEntitySet<Artist>
    {
        Task<IEnumerable<Artist>> FindAsync(Artist.QueryParameter parameter);
    }
}