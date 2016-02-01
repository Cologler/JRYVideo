using Jasily.ComponentModel;
using JryVideo.Core;
using JryVideo.Core.Managers;

namespace JryVideo.Common
{
    public class JryVideoViewModel<T> : JasilyViewModel<T>
    {
        public JryVideoViewModel(T source)
            : base(source)
        {
        }

        public DataCenter Managers => JryVideoCore.Current.CurrentDataCenter;
    }
}