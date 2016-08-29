using Jasily.ComponentModel;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;
using System.Collections.Generic;

namespace JryVideo.Data.MongoDb
{
    public class MongoVideoRoleCollectionDataSource : MongoJryEntitySet<VideoRoleCollection, VideoRoleCollection.QueryParameter>, IVideoRoleCollectionSet
    {
        public MongoVideoRoleCollectionDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<VideoRoleCollection> collection)
            : base(engine, collection)
        {
        }

        protected override IEnumerable<FilterDefinition<VideoRoleCollection>> BuildFilters(VideoRoleCollection.QueryParameter parameter)
        {
            if (parameter.ActorId != null)
            {
                yield return
                    Builders<VideoRoleCollection>.Filter.Eq(
                        PropertySelector<VideoRoleCollection>.Start(z => z)
                            .SelectMany(z => z.MajorRoles)
                            .Select(z => z.ActorId).ToString(),
                        parameter.ActorId) | 
                    Builders<VideoRoleCollection>.Filter.Eq(
                        PropertySelector<VideoRoleCollection>.Start(z => z)
                            .SelectMany(z => z.MinorRoles)
                            .Select(z => z.ActorId).ToString(),
                        parameter.ActorId);
            }
        }
    }
}