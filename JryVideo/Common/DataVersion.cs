using System.Diagnostics;
using System.Linq;
using JryVideo.Model;

namespace JryVideo.Common
{
    public sealed class DataVersion<T> where T : JryObject
    {
        private bool isObsolete;
        private readonly T item;
        private int version;

        public DataVersion(T item, int initVersion)
        {
            Debug.Assert(item != null);
            this.item = item;
            this.version = initVersion;
        }

        public bool IsObsolete()
        {
            if (this.isObsolete) return true;
            var journal = JryVideoViewModel.GetManagers(null).Journal;
            if (this.version == journal.Version) return false;
            int @new;
            var logs = journal.GetChanged(this.version, out @new);
            if (logs.Any(z => z.IsObsolete(this.item)))
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
    }
}