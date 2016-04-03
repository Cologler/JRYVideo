using System.Collections.Generic;
using System.Linq;
using JryVideo.Core.Managers.Journals;
using JryVideo.Model;

namespace JryVideo.Common
{
    public sealed class FlagTypeDataVersion : DataVersion
    {
        private readonly JryFlagType flag;

        public FlagTypeDataVersion(JryFlagType flag, int initVersion)
            : base(initVersion)
        {
            this.flag = flag;
        }

        protected override bool IsObsoleteCore(IEnumerable<IDataJournal> logs)
            => logs.Any(z => z.IsObsolete(this.flag));
    }
}