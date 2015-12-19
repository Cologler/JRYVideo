using System.IO;

namespace JryVideo.Viewer.FilesViewer
{
    public sealed class FileItemViewModel : PathItemViewModel
    {
        public FileItemViewModel(string path)
            : base(path)
        {
            this.IsExists = File.Exists(path);
        }
    }
}