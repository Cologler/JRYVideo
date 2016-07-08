using System;
using System.Attributes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JryVideo.Model.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public abstract class JryObject : IObject
    {
        [BsonIgnoreIfDefault]
        public int Version { get; set; }

        [Cloneable]
        public string Id { get; set; }

        [Cloneable]
        public DateTime Created { get; set; }

        [BsonIgnoreIfDefault]
        public string Description { get; set; }

        public virtual void BuildMetaData(bool isForce = false)
        {
            if (!isForce && this.Id != null) throw new Exception("can not rebuild meta data.");

            this.BuildId();
            this.Created = DateTime.UtcNow;
        }

        public bool IsMetaDataBuilded()
        {
            return !String.IsNullOrWhiteSpace(this.Id);
        }

        protected virtual void BuildId()
        {
            this.Id = Guid.NewGuid().ToString().ToUpper();
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
            if (string.IsNullOrWhiteSpace(this.Description)) this.Description = null;
        }

        /// <summary>
        /// return 'type [id]'
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{this.GetType().Name} [{this.Id}]";

        protected static List<string> CombineStrings(List<string> first, List<string> second)
        {
            if (first == null || second == null)
            {
                return first?.ToList() ?? second?.ToList();
            }

            return first.Concat(second).Distinct().ToList();
        }

        protected static bool CanCombineField(string left, string right)
        {
            return left == null || right == null || left == right;
        }
    }
}