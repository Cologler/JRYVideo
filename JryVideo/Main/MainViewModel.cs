using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using JryVideo.Core;
using JryVideo.Data;

namespace JryVideo.Main
{
    public class MainViewModel : JasilyViewModel
    {
        private DataModeViewModel selectedMode;

        public MainViewModel()
        {
            this.VideosViewModel = new MainSeriesItemViewerViewModel();
            this.ModeCollection = new ObservableCollection<DataModeViewModel>()
            {
                new DataModeViewModel(JryVideoDataSourceProviderManagerMode.Public),
                new DataModeViewModel(JryVideoDataSourceProviderManagerMode.Private)
            };
            this.selectedMode = this.ModeCollection[0];
        }

        public MainSeriesItemViewerViewModel VideosViewModel { get; private set; }

        public async Task LoadAsync()
        {
            this.BeginUpdateDataSouce();
        }

        public ObservableCollection<DataModeViewModel> ModeCollection { get; private set; }

        public DataModeViewModel SelectedMode
        {
            get { return this.selectedMode; }
            set
            {
                if (this.SetPropertyRef(ref this.selectedMode, value))
                {
                    JryVideoCore.Current.Switch(value.Source);
                    this.BeginUpdateDataSouce();
                }
            }
        }

        private void Switch()
        {
            
        }

        private async void BeginUpdateDataSouce()
        {
            await this.VideosViewModel.RefreshAsync();
        }

        public async Task LastPageAsync()
        {
            this.VideosViewModel.PageIndex--;
            await this.VideosViewModel.RefreshAsync();
        }

        public async Task NextPageAsync()
        {
            this.VideosViewModel.PageIndex++;
            await this.VideosViewModel.RefreshAsync();
        }

        public async Task SetOnlyTrackingAsync()
        {
            await this.VideosViewModel.RefreshAsync();
        }

        public async Task UnsetOnlyTrackingAsync()
        {
            await this.VideosViewModel.RefreshAsync();
        }
    }
}