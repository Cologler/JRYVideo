using System.ComponentModel;
using Jasily.ComponentModel;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class FlagViewModel : JasilyViewModel<JryFlag>
    {
        public FlagViewModel(JryFlag source)
            : base(source)
        {
        }
    }
}