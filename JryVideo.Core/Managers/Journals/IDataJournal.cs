using JryVideo.Model;
using System;

namespace JryVideo.Core.Managers.Journals
{
    public interface IDataJournal
    {
        DataJournalType Type { get; }

        bool IsObsolete(Type type);

        bool IsObsolete(JryObject obj);

        bool IsObsolete(JryFlagType flag);
    }
}