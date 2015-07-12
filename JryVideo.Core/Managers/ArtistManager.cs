using System.Data;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class ArtistManager : JryObjectManager<JryArtist, IJasilyEntitySetProvider<JryArtist, string>>
    {
        public ArtistManager(IJasilyEntitySetProvider<JryArtist, string> source)
            : base(source)
        {
        }
    }
}