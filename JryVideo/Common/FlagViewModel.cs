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

        public string NameWithCount => $"{this.Source.Value} ({this.Source.Count})";
    }
}