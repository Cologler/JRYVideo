using System;
using System.Attributes;
using System.Collections.Generic;

namespace JryVideo.Model
{
    public abstract class JryObject
    {
        [Cloneable]
        public string Id { get; set; }

        [Cloneable]
        public DateTime Created { get; set; }

        protected virtual T InitializeInstance<T>(T obj)
            where T : JryObject
        {
            obj.Id = this.BuildId();
            obj.Created = DateTime.UtcNow;
            return obj;
        }

        /// <summary>
        /// Guid.NewGuid().ToString().ToUpper()
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildId()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }

        public virtual IEnumerable<string> CheckError()
        {
            if (String.IsNullOrWhiteSpace(this.Id)) yield return "error Id";

            if (this.Created == DateTime.MinValue) yield return "error Created";
        }
    }
}