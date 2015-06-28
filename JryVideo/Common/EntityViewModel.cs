using System.ComponentModel;
using JryVideo.Model;

namespace JryVideo.Common
{
    public sealed class EntityViewModel : JasilyViewModel<JryEntity>
    {
        public EntityViewModel(JryEntity source)
            : base(source)
        {
        }
    }
}