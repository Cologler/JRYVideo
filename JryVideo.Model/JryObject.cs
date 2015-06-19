using System;
using System.Collections.Generic;

namespace JryVideo.Model
{
    public abstract class JryObject
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        protected virtual T InitializeInstance<T>(T obj)
            where T : JryObject
        {
            obj.Id = Guid.NewGuid();
            obj.Created = DateTime.UtcNow;
            return obj;
        }

        public virtual IEnumerable<string> CheckError()
        {
            if (this.Id == Guid.Empty) yield return "error Id";

            if (this.Created == DateTime.MinValue) yield return "error Created";
        }
    }
}