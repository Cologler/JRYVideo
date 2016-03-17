using System.IO;

namespace JryVideo.Viewer.FilesViewer
{
    public sealed class FileItemViewModel : PathItemViewModel
    {
        public FileItemViewModel(string path)
            : base(path)
        {
        }

        public override bool IsExists => File.Exists(this.Source);
    }
}