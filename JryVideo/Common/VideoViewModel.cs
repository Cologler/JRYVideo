using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Core;

namespace JryVideo.Common
{
    public sealed class VideoViewModel : JasilyViewModel<Model.JryVideo>
    {
        public JasilyCollectionView<EntityViewModel> EntityViews { get; private set; }

        public VideoViewModel(Model.JryVideo source)
            : base(source)
        {
            this.EntityViews = new JasilyCollectionView<EntityViewModel>();
            this.EntityViews.Collection.AddRange(source.Entities.Select(z => new EntityViewModel(z)));
        }

        public async Task<bool> RemoveAsync(EntityViewModel entity)
        {
            var manager = JryVideoCore.Current.CurrentDataCenter.VideoManager.GetEntityManager(this.Source);

            if (await manager.RemoveAsync(entity.Source.Id))
            {
                this.EntityViews.Collection.Remove(entity);

                return true;
            }
            return false;
        }
    }
}