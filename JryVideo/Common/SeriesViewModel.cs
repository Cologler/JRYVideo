using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasily.ComponentModel;
using JryVideo.Editors.SeriesEditor;
using JryVideo.Model;
using System.Windows;

namespace JryVideo.Common
{
    public sealed class SeriesViewModel : JasilyViewModel<JrySeries>
    {
        private readonly List<VideoInfoViewModel> videoViewModels = new List<VideoInfoViewModel>();

        public SeriesViewModel(JrySeries source)
            : base(source)
        {
            this.NameViewModel = new NameableViewModel<JrySeries>(source);

            this.videoViewModels.AddRange(source.Videos.Select(jryVideo => new VideoInfoViewModel(this, jryVideo)));
        }

        public NameableViewModel<JrySeries> NameViewModel { get; }

        public IEnumerable<VideoInfoViewModel> VideoViewModels => this.videoViewModels;

        /// <summary>
        /// the method will call PropertyChanged for each property which has [NotifyPropertyChanged]
        /// </summary>
        public override void RefreshProperties()
        {
            base.RefreshProperties();
            this.NameViewModel.RefreshProperties();
        }

        /// <summary>
        /// like ({0} videos) this.DisplayName
        /// </summary>
        [NotifyPropertyChanged]
        public string DisplayNameInfo => $"({this.Source.Videos.Count} videos) {string.Join(" / ", this.Source.Names)}";

        public bool OpenEditorWindows(Window parent)
        {
            var dlg = new SeriesEditorWindow(this.Source)
            {
                Owner = parent
            };
            if (dlg.ShowDialog() == true)
            {
                this.RefreshProperties();
                return true;
            }
            return false;
        }

        public async Task AutoCompleteAsync()
        {
            if (this.Source.TheTVDBId.IsNullOrWhiteSpace())
            {
                var imdbId = this.Source.GetValidImdbId();
                var client = this.GetTVDBClient();
                if (client != null && imdbId != null)
                {
                    var series = (await client.GetSeriesByImdbIdAsync(imdbId)).FirstOrDefault();
                    if (series != null)
                    {
                        this.Source.TheTVDBId = series.SeriesId;
                        await this.GetManagers().SeriesManager.UpdateAsync(this.Source);
                    }
                }
            }
        }
    }
}