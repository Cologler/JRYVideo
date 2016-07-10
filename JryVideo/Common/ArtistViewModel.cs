using System.Threading.Tasks;
using System.Windows;
using Jasily.ComponentModel;
using JryVideo.Common.Windows;
using JryVideo.Core.Douban;
using JryVideo.Core.Managers;
using JryVideo.Core.Models;
using JryVideo.Editors.ArtistEditor;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

namespace JryVideo.Common
{
    public sealed class ArtistViewModel : JasilyViewModel<Artist>
    {
        private static readonly AutoGenerateCoverProvider CoverProvider = new AutoGenerateCoverProvider();

        public ArtistViewModel(Artist source)
            : base(source)
        {
            this.NameView = new NameableViewModel<Artist>(source);
            this.CoverViewModel = new CoverViewModel(source)
            {
                AutoGenerateCoverProvider = CoverProvider
            };
        }

        public string Name => string.Join(" / ", this.Source.Names);

        public NameableViewModel<Artist> NameView { get; }

        public CoverViewModel CoverViewModel { get; }

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

        private class AutoGenerateCoverProvider : IAutoGenerateCoverProvider
        {
            /// <summary>
            /// return true if success.
            /// </summary>
            /// <param name="dataCenter"></param>
            /// <param name="source"></param>
            /// <returns></returns>
            public async Task<bool> GenerateAsync(DataCenter dataCenter, ICoverParent source)
            {
                var item = (Artist)source;
                var doubanId = item.DoubanId;
                if (string.IsNullOrWhiteSpace(doubanId)) return false;
                var info = await DoubanHelper.TryGetArtistInfoAsync(doubanId);
                if (info == null) return false;
                var url = info.GetLargeImageUrl();
                var coverBuilder = CoverBuilder.CreateArtist(item);
                coverBuilder.Uri.Add(url);
                return await dataCenter.CoverManager.BuildCoverAsync(coverBuilder);
            }
        }
    }
}