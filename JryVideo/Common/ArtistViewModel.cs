using JryVideo.Model;
using System.Threading.Tasks;

namespace JryVideo.Common
{
    public class ArtistViewModel : HasCoverViewModel<JryArtist>
    {
        public ArtistViewModel(JryArtist source)
            : base(source)
        {
        }

        public string Name => string.Join(" / ", this.Source.Names);

        protected override async Task<bool> TryAutoAddCoverAsync()
        {
            if (this.Source.DoubanId == null) return false;

            //var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;

            //var guid = await coverManager.AutoGenerateCoverAsync(JryCoverType.Artist, this.Source.DoubanId);

            //if (guid == null) return false;

            //this.Source.CoverId = guid;

            //var artistManager = JryVideoCore.Current.CurrentDataCenter.ArtistManager;
            //await artistManager.UpdateAsync(this.Source);
            return true;
        }
    }
}