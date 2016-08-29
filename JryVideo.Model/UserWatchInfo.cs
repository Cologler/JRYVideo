using System.Collections.Generic;
using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed class UserWatchInfo : VideoInfoAttached
    {
        /// <summary>
        /// 尽量排序，但是不一定排序
        /// </summary>
        [BsonIgnoreIfDefault]
        public List<int> Watcheds { get; set; }
    }
}