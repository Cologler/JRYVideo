using Jasily.ComponentModel;
using JryVideo.Data;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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

        public void LoadAsync() => this.BeginUpdateDataSouce();

        public ObservableCollection<DataModeViewModel> ModeCollection { get; private set; }

        public DataModeViewModel SelectedMode
        {
            get { return this.selectedMode; }
            set { this.SetPropertyRef(ref this.selectedMode, value); }
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