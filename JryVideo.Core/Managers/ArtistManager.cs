using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class ArtistManager : JryObjectManager<JryArtist, IDataSourceProvider<JryArtist>>
    {
        public ArtistManager(IDataSourceProvider<JryArtist> source)
            : base(source)
        {
        }
    }
}