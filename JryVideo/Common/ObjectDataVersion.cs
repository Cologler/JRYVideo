using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JryVideo.Core.Managers.Journals;
using JryVideo.Model;

namespace JryVideo.Common
{
    public sealed class ObjectDataVersion<T> : DataVersion where T : JryObject
    {
        private readonly T item;

        public ObjectDataVersion(T item, int initVersion)
            : base(initVersion)
        {
            Debug.Assert(item != null);
            this.item = item;
        }

        protected override bool IsObsoleteCore(IEnumerable<IDataJournal> logs)
            => logs.Any(z => z.IsObsolete(this.item));
    }
}