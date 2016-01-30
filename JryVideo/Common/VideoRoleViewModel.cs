using JryVideo.Model;
using System.Threading.Tasks;

namespace JryVideo.Common
{
    public class VideoRoleViewModel : HasCoverViewModel<JryVideoRole>
    {
        public VideoRoleViewModel(JryVideoRole source)
            : base(source)
        {
            this.NameViewModel = new NameableViewModel<JryVideoRole>(source);
        }

        public NameableViewModel<JryVideoRole> NameViewModel { get; }

        /// <summary>
        /// the method will call PropertyChanged for each property which has [NotifyPropertyChanged]
        /// </summary>
        public override void RefreshProperties()
        {
            base.RefreshProperties();
            this.NameViewModel.RefreshProperties();
        }

        protected override Task<bool> TryAutoAddCoverAsync()
        {
            return Task.FromResult(false);
        }
    }
}