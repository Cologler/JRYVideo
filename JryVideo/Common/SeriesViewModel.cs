using Jasily.ComponentModel;
using JryVideo.AutoComplete;
using JryVideo.Common.Dialogs;
using JryVideo.Editors.SeriesEditor;
using JryVideo.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace JryVideo.Common
{
    public sealed class SeriesViewModel : JasilyViewModel<JrySeries>
    {
        private bool isObsolete;
        private int version;
        private static readonly RefreshPropertiesMapper Mapper = new RefreshPropertiesMapper(typeof(SeriesViewModel));
        private readonly List<VideoInfoViewModel> videoViewModels = new List<VideoInfoViewModel>();

        public SeriesViewModel(JrySeries source, int version)
            : base(source)
        {
            this.version = version;
            this.PropertiesMapper = Mapper;
            this.NameViewModel = new NameableViewModel<JrySeries>(source);

            this.videoViewModels.AddRange(source.Videos.Select(z => new VideoInfoViewModel(this, z)));
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
            if (this.IsObsolete())
            {
                parent.ShowJryVideoMessage("error", "data was obsolete, please refresh.");
                return false;
            }

            var dlg = new SeriesEditorWindow(this)
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

        public bool IsObsolete()
        {
            if (this.isObsolete) return true;
            var journal = this.GetManagers().Journal;
            if (this.version == journal.Version) return false;
            int @new;
            var logs = journal.GetChanged(this.version, out @new);
            if (logs.Any(z => z.IsObsolete(typeof(JrySeries))))
            {
                this.isObsolete = true;
                return true;
            }
            else
            {
                this.version = @new;
                return false;
            }
        }

        public async Task AutoCompleteAsync()
        {
            await new SeriesAutoComplete().AutoCompleteAsync(this.GetManagers().SeriesManager, this.Source);
        }

        public static implicit operator JrySeries(SeriesViewModel viewModel) => viewModel?.Source;
    }
}