using JryVideo.Data.DataSources;
using JryVideo.Model;
using System.Data;

namespace JryVideo.Core.Managers
{
    public sealed class EntityManager : JryObjectManager<JryEntity, IJasilyEntitySetProvider<JryEntity, string>>
    {
        private readonly VideoManager.EntityJryEntitySetSet provider;
        
        internal EntityManager(VideoManager.EntityJryEntitySetSet source)
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