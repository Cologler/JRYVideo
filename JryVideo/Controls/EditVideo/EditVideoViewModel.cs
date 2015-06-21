using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Model;

namespace JryVideo.Controls.EditVideo
{
    public class EditVideoViewModel : JasilyViewModel
    {
        private ImageViewModel _imageViewModel;
        private JryCounter _selectedType;

        public EditVideoViewModel()
        {
            this.TypeCollection = new ObservableCollection<JryCounter>();
        }

        public ImageViewModel ImageViewModel
        {
            get { return this._imageViewModel; }
            private set { this.SetPropertyRef(ref this._imageViewModel, value); }
        }

        public ObservableCollection<JryCounter> TypeCollection { get; private set; }

        public JryCounter SelectedType
        {
            get { return this._selectedType; }
            set { this.SetPropertyRef(ref this._selectedType, value); }
        }

        public async Task LoadAsync()
        {
            var types = (await JryVideoCore.Current.CurrentDataCenter.CounterManager.LoadAsync(JryCounterType.VideoType)).ToArray();

            this.TypeCollection.AddRange(types);

            this.SelectedType = types.FirstOrDefault();
        }
    }
}