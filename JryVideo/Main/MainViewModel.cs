using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
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
            this.ModeCollection = new ObservableCollection<DataModeViewModel>();
        }

        public MainSeriesItemViewerViewModel VideosViewModel { get; private set; }

        public async Task LoadAsync()
        {
            this.ModeCollection.Add(new DataModeViewModel(JryVideoDataSourceProviderManagerMode.Public));
            this.ModeCollection.Add(new DataModeViewModel(JryVideoDataSourceProviderManagerMode.Private));
            this.SelectedMode = this.ModeCollection[0];

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

        private async void BeginUpdateDataSouce()
        {
            await this.VideosViewModel.LoadAsync();
        }
    }
}