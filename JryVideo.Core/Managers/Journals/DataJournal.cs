using JryVideo.Model;
using System.Collections.Generic;
using System.Linq;

namespace JryVideo.Core.Managers.Journals
{
    public sealed class DataJournal
    {
        private readonly List<IDataJournal> journals = new List<IDataJournal>();

        public int Version
        {
            get
            {
                lock (this.journals)
                {
                    return this.journals.Count;
                }
            }
        }

        public IEnumerable<IDataJournal> GetChanged(int from, out int to)
        {
            lock (this.journals)
            {
                to = this.journals.Count;
                return this.journals.Skip(from).ToArray();
            }
        }

        public void Initialize(DataCenter dataCenter)
        {
            dataCenter.FlagManager.ItemCreated += this.FlagManager_ItemCreated;
            dataCenter.FlagManager.FlagChanged += this.FlagManager_FlagChanged;
        }

        private void FlagManager_ItemCreated(object sender, JryFlag e)
        {
            lock (this.journals)
            {
                this.journals.Add(new FlagCreatedJournal(e.Type, e.Value));
            }
        }

        private void FlagManager_FlagChanged(object sender, System.EventArgs<JryFlagType, string, string> e)
        {
            lock (this.journals)
            {
                this.journals.Add(new FlagChangedJournal(e));
            }
        }
    }
}