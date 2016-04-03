using System;
using System.Diagnostics;
using JryVideo.Model;

namespace JryVideo.Core.Managers.Journals
{
    public sealed class FlagCreatedJournal : IDataJournal
    {
        public FlagCreatedJournal(JryFlagType type, string value)
        {
            this.FlagType = type;
        }

        public DataJournalType Type => DataJournalType.FlagCreated;

        public bool IsObsolete(Type type)
        {
            return type == typeof(JryFlagType);
        }

        public bool IsObsolete(JryObject obj)
        {
            Debug.Assert(obj != null);
            return this.IsObsolete(obj.GetType());
        }

        public bool IsObsolete(JryFlagType flag) => this.FlagType == flag;

        public JryFlagType FlagType { get; }
    }
}