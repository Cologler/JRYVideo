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

        public virtual void BuildMetaData(bool isForce = false)
        {
            if (!isForce && this.Id != null) throw new Exception("can not rebuild meta data.");

            this.Id = this.BuildId();
            this.Created = DateTime.UtcNow;
        }

        public bool IsMetaDataBuilded()
        {
            return !String.IsNullOrWhiteSpace(this.Id);
        }

        /// <summary>
        /// Guid.NewGuid().ToString().ToUpper()
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildId()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }

        public virtual IEnumerable<JryInvalidError> CheckError()
        {
            if (String.IsNullOrWhiteSpace(this.Id) || this.Created == DateTime.MinValue)
                throw new Exception("forgot to build meta data.");

            yield break;
        }
    }
}