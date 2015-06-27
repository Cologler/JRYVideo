using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Editors.CoverEditor;
using JryVideo.Model;

namespace JryVideo.Controls.EditVideo
{
    public class EditVideoViewModel : EditorItemViewModel<Model.JryVideoInfo>
    {
        private ImageViewModel imageViewModel;
        private JryFlag selectedType;
        private CoverEditorViewModel cover;

        public EditVideoViewModel()
        {
            this.TypeCollection = new ObservableCollection<JryFlag>();
        }

        public ImageViewModel ImageViewModel
        {
            get { return this.imageViewModel; }
            private set { this.SetPropertyRef(ref this.imageViewModel, value); }
        }

        public ObservableCollection<JryFlag> TypeCollection { get; private set; }

        public JryFlag SelectedType
        {
            get { return this.selectedType; }
            set { this.SetPropertyRef(ref this.selectedType, value); }
        }

        public CoverEditorViewModel Cover
        {
            get { return this.cover; }
            set { this.ImageViewModel = (this.cover = value) != null ? value.ImageViewModel : null; }
        }

        public async Task LoadAsync()
        {
            var types = (await JryVideoCore.Current.CurrentDataCenter.FlagManager.LoadAsync(JryFlagType.VideoType)).ToArray();

            this.TypeCollection.AddRange(types);

            this.SelectedType = types.FirstOrDefault();

            if (this.Source != null && this.Source.CoverId != null)
            {
                var cover = await JryVideoCore.Current.CurrentDataCenter.CoverManager.FindAsync(this.Source.CoverId);

                if (cover != null)
                {
                    this.ImageViewModel = ImageViewModel.Build(cover.BinaryData);
                }
            }
        }
    }
}