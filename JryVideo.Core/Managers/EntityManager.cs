using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public sealed class EntityManager : JryObjectManager<JryEntity, IDataSourceProvider<JryEntity>>
    {
        private readonly VideoManager.EntityDataSourceProvider provider;
        
        internal EntityManager(VideoManager.EntityDataSourceProvider source)
            : base(source)
        {
            this.provider = source;
        }

        public bool IsExists(JryEntity entity)
        {
            return this.provider.IsExists(entity);
        }
    }
}