using System.ComponentModel;
using JryVideo.Model;

namespace JryVideo.Selectors.FlagSelector
{
    public class FlagViewModel : JasilyViewModel<JryFlag>
    {
        public FlagViewModel(JryFlag source)
            : base(source)
        {
        }
    }
}