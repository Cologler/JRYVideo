using Jasily.ComponentModel;

namespace JryVideo.Viewer.FilesViewer
{
    public class PathItemViewModel : JasilyViewModel<string>
    {
        protected PathItemViewModel(string path)
            : base(path)
        {
        }

        public bool IsExists { get; protected set; }
    }
}