using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Core;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace JryVideo.Common
{
    public sealed class VideoViewModel : JasilyViewModel<Model.JryVideo>
    {
        private bool isObsolete;

        public JasilyCollectionView<EntityViewModel> EntityViews { get; private set; }

        public VideoViewModel(Model.JryVideo source)
            : base(source)
        {
            this.EntityViews = new JasilyCollectionView<EntityViewModel>();
            this.EntityViews.Collection.AddRange(source.Entities.Select(z => new EntityViewModel(z)));
        }

        public async Task<bool> RemoveAsync(EntityViewModel entity)
        {
            var manager = this.GetManagers().VideoManager.GetEntityManager(this.Source);
            Log.BeginWrite($"DELETE {entity.Source.ObjectToJson()} FROM {this.Source.Id}");
            if (!await manager.RemoveAsync(entity.Source.Id)) return false;
            this.EntityViews.Collection.Remove(entity);
            return true;
        }

        public bool IsObsolete => this.isObsolete;

        public void SetObsoleted() => this.isObsolete = true;
    }
}