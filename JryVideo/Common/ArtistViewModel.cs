using JryVideo.Common.Windows;
using JryVideo.Editors.ArtistEditor;
using JryVideo.Model;
using System.Threading.Tasks;
using System.Windows;
using JryVideo.Core.Douban;
using JryVideo.Core.Models;

namespace JryVideo.Common
{
    public sealed class ArtistViewModel : HasCoverViewModel<Artist>
    {
        public ArtistViewModel(Artist source)
            : base(source)
        {
            this.NameView = new NameableViewModel<Artist>(source);
        }

        public string Name => string.Join(" / ", this.Source.Names);

        public NameableViewModel<Artist> NameView { get; }

        /// <summary>
        /// the method will call PropertyChanged for each property which has [NotifyPropertyChanged]
        /// </summary>
        public override void RefreshProperties()
        {
            base.RefreshProperties();
            this.NameView.RefreshProperties();
        }

        public async Task EditAsync(Window parent)
        {
            var page = new ArtistEditorPage();
            page.ViewModel.ReadFromObject(this.Source);
            if (page.ShowDialog(parent) == true)
            {
                page.ViewModel.WriteToObject(this.Source);
                this.RefreshProperties();
                await this.GetManagers().ArtistManager.UpdateAsync(this.Source);
            }
        }

        protected override async Task<bool> TryAutoAddCoverAsync()
        {
            var doubanId = this.Source.DoubanId;
            if (string.IsNullOrWhiteSpace(doubanId)) return false;
            var info = await DoubanHelper.TryGetArtistInfoAsync(doubanId);
            if (info == null) return false;
            var url = info.GetLargeImageUrl();
            var coverBuilder = CoverBuilder.CreateArtist(doubanId, url, this.Source);
            var id = await this.GetManagers().CoverManager.BuildCoverAsync(coverBuilder);
            if (id == null) return false;
            this.Source.CoverId = id;
            await this.GetManagers().ArtistManager.UpdateAsync(this.Source);
            return true;
        }
    }
}