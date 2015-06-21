using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Model;

namespace JryVideo.Add.SelectVideo
{
    public class SelectVideoViewModel : JasilyViewModel<JrySeries>
    {
        private VideoViewModel _selected;

        public ObservableCollection<VideoViewModel> VideoCollection { get; private set; }

        public async Task LoadAsync()
        {
            var series = this.Source;

            this.VideoCollection.AddRange(
                await Task.Run(() => VideoViewModel.Create(series)));
        }

        public SelectVideoViewModel(JrySeries source)
            : base(source)
        {
            this.VideoCollection = new ObservableCollection<VideoViewModel>();
        }

        public VideoViewModel Selected
        {
            get { return this._selected; }
            set { this.SetPropertyRef(ref this._selected, value); }
        }
    }
}