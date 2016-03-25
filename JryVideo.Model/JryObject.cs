using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Attributes;
using System.Data;
using System.Diagnostics;

namespace JryVideo.Model
{
    public abstract class JryObject : IJasilyEntity<string>
    {
        [Cloneable]
        public string Id { get; set; }

        [Cloneable]
        public DateTime Created { get; set; }

        [BsonIgnoreIfDefault]
        public string Desciption { get; set; }

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

        public bool HasError()
        {
            if (this.InnerTestHasError())
            {
                JasilyLogger.Current.WriteLine(JasilyLogger.LoggerMode.Debug, "object has error.", this.GetType());
                JasilyLogger.Current.WriteLine(JasilyLogger.LoggerMode.Debug, this.Print() + "\r\n", this.GetType());
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual bool InnerTestHasError()
        {
            if (String.IsNullOrWhiteSpace(this.Id) || this.Created == DateTime.MinValue)
                throw new Exception("forgot to build meta data.");

            return false;
        }

        public virtual void Saving()
        {
            if (string.IsNullOrWhiteSpace(this.Desciption)) this.Desciption = null;
        }

        protected static bool CombineEquals(string left, string right)
            => left == null || right == null || left == right;

        /// <summary>
        /// return 'type [id]'
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{this.GetType().Name} [{this.Id}]";
    }
}