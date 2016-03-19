using Jasily.ComponentModel;
using JryVideo.Core.Managers;
using JryVideo.Model;

namespace JryVideo.Selectors.FlagSelector
{
    public class FlagViewModel : JasilyViewModel<JryFlag>
    {
        public FlagViewModel(JryFlag source)
            : base(source)
        {
        }

        public bool CanReplace => FlagManager.CanReplace(this.Source.Type);
    }
}