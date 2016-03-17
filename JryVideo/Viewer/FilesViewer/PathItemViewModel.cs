using Jasily.ComponentModel;

namespace JryVideo.Viewer.FilesViewer
{
    public abstract class PathItemViewModel : JasilyViewModel<string>
    {
        protected PathItemViewModel(string path)
            : base(path)
        {
        }

        public abstract bool IsExists { get; }
    }
}