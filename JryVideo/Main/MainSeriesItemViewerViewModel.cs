using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Common;
using JryVideo.Viewer.SeriesItemViewer;

namespace JryVideo.Main
{
    public class MainSeriesItemViewerViewModel : SeriesItemViewerViewModel
    {
        public async override Task LoadAsync()
        {
            var manager = JryVideo.Core.JryVideoCore.Current.CurrentDataCenter.SeriesManager;

            var series = await manager.LoadAsync();

            this.VideosObservableCollection.AddRange(
                await Task.Run(() => series.SelectMany(VideoViewModel.Create).ToArray()));
        }
    }
}