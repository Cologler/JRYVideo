using JryVideo.Model;
using System;
using System.Diagnostics;

namespace JryVideo.Core.Managers.Journals
{
    public sealed class FlagChangedJournal : IDataJournal
    {
        public FlagChangedJournal(EventArgs<JryFlagType, string, string> e)
        {
            this.FlagType = e.Value1;
        }

        public DataJournalType Type => DataJournalType.FlagChanged;

        public bool IsObsolete(Type type)
        {
            if (type == typeof(JryEntity) && (int)this.FlagType > 20)
                return true;

            if ((type == typeof(JrySeries) || type == typeof(JryVideoInfo)) && (int)this.FlagType < 20)
                return true;

            return false;
        }

        public bool IsObsolete(JryObject obj)
        {
            Debug.Assert(obj != null);
            return this.IsObsolete(obj.GetType());
        }

        public JryFlagType FlagType { get; }
    }
}