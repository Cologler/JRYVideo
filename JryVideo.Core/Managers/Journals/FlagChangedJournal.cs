using System;
using System.Diagnostics;
using JryVideo.Model;

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
            if (type == typeof(Resource) && (int)this.FlagType > 20)
                return true;

            if ((type == typeof(Series) || type == typeof(JryVideoInfo)) && (int)this.FlagType < 20)
                return true;

            return false;
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