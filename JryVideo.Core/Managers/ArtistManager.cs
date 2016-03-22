using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class ArtistManager : JryObjectManager<Artist, IArtistSet>
    {
        public ArtistManager(IArtistSet source)
            : base(source)
        {
        }
    }
}