using Jasily.ComponentModel;
using JryVideo.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace JryVideo.Main
{
    public class MainViewModel : JasilyViewModel
    {
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
        }

        public MainSeriesItemViewerViewModel VideosViewModel { get; } = new MainSeriesItemViewerViewModel();

        public async void ReloadAsync() => await this.VideosViewModel.ReloadAsync();

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
    }
}