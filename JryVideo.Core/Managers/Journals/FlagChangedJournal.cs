using JryVideo.Model;
using System;

namespace JryVideo.Core.Managers.Journals
{
    public sealed class FlagChangedJournal : IDataJournal
    {
        public FlagChangedJournal(EventArgs<JryFlagType, string, string> e)
        {
            this.FlagType = e.Value1;
        }

        public DataJournalType Type => DataJournalType.FlagChanged;

        public JryFlagType FlagType { get; }
    }
}