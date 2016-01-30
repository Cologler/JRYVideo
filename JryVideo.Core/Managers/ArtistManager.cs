using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class ArtistManager : JryObjectManager<JryArtist, IArtistSet>
    {
        public ArtistManager(IArtistSet source)
            : base(source)
        {
        }
    }
}