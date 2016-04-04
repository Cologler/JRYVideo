using Jasily.ComponentModel;
using JryVideo.Common;
using JryVideo.Data;
using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace JryVideo.Main
{
    public class MainViewModel : JasilyViewModel
    {
        public bool isInitializeLoaded;
        private NameValuePair<JryVideoDataSourceProviderManagerMode> selectedMode;

        public MainViewModel()
        {
            this.ModeCollection = new ObservableCollection<NameValuePair<JryVideoDataSourceProviderManagerMode>>()
            {
                JryVideoDataSourceProviderManagerMode.Public.WithName(
                    nameof(JryVideoDataSourceProviderManagerMode.Public)),
                JryVideoDataSourceProviderManagerMode.Private.WithName(
                    nameof(JryVideoDataSourceProviderManagerMode.Private)),
            };
            this.selectedMode = this.ModeCollection[0];

            this.GetAgent().FlagChanged += this.MainViewModel_FlagChanged;
        }

        private void MainViewModel_FlagChanged(object sender, EventArgs<Tuple<JryFlagType, string, string>> e)
        {
            var type = e.Value.Item1;
            if ((int)type < 20)
            {
                this.GetUIDispatcher().BeginInvoke(() => this.VideosViewModel.AllObsoleted());
            }
        }

        public MainSeriesItemViewerViewModel VideosViewModel { get; } = new MainSeriesItemViewerViewModel();

        public async void ReloadAsync()
        {
            await this.VideosViewModel.ReloadAsync();
            this.ReloadGrouping();
            this.isInitializeLoaded = true;
        }

        public async Task ReloadIfInitializedAsync()
        {
            if (this.isInitializeLoaded)
            {
                await this.VideosViewModel.ReloadAsync();
                this.ReloadGrouping();
            }
        }

        public async void ReloadGrouping()
        {
            await this.Grouping.ReloadFlagsAsync(
                JryFlagType.SeriesTag,
                JryFlagType.VideoYear,
                JryFlagType.VideoTag,
                JryFlagType.VideoType);
        }

        public ObservableCollection<NameValuePair<JryVideoDataSourceProviderManagerMode>> ModeCollection { get; }

        public NameValuePair<JryVideoDataSourceProviderManagerMode> SelectedMode
        {
            get { return this.selectedMode; }
            set { this.SetPropertyRef(ref this.selectedMode, value); }
        }

        public async Task LastPageAsync()
        {
            this.VideosViewModel.PageIndex--;
            await this.VideosViewModel.ReloadAsync();
        }

        public async Task NextPageAsync()
        {
            this.VideosViewModel.PageIndex++;
            await this.VideosViewModel.ReloadAsync();
        }

        public VideoGroupingViewModel Grouping { get; } = new VideoGroupingViewModel();

        public void OnClickGrouping(object item)
        {
            var flag = item as FlagViewModel;
            if (flag != null)
            {
                switch (flag.Source.Type)
                {
                    case JryFlagType.SeriesTag:
                    case JryFlagType.VideoTag:
                        this.VideosViewModel.SearchText = "tag:" + flag.Source.Value;
                        break;

                    case JryFlagType.VideoYear:
                        this.VideosViewModel.SearchText = "year:" + flag.Source.Value;
                        break;

                    case JryFlagType.VideoType:
                        this.VideosViewModel.SearchText = "type:" + flag.Source.Value;
                        break;


                    case JryFlagType.EntityResolution:
                    case JryFlagType.EntityQuality:
                    case JryFlagType.EntityExtension:
                    case JryFlagType.EntityFansub:
                    case JryFlagType.EntitySubTitleLanguage:
                    case JryFlagType.EntityTrackLanguage:
                    case JryFlagType.EntityAudioSource:
                    case JryFlagType.EntityTag:
                        throw new NotSupportedException();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return;
            }

            var star = (int)item;
            this.VideosViewModel.SearchText = "star:" + star;
        }
    }
}