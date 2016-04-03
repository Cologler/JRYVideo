using JryVideo.Core.Managers.Journals;
using System.Collections.Generic;

namespace JryVideo.Common
{
    public abstract class DataVersion
    {
        private bool isObsolete;
        private int version;

        protected DataVersion(int initVersion)
        {
            this.version = initVersion;
        }

        public bool IsObsolete()
        {
            if (this.isObsolete) return true;
            var journal = JryVideoViewModel.GetManagers(null).Journal;
            if (this.version == journal.Version) return false;
            int @new;
            var logs = journal.GetChanged(this.version, out @new);
            if (this.IsObsoleteCore(logs))
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

        protected abstract bool IsObsoleteCore(IEnumerable<IDataJournal> logs);
    }
}