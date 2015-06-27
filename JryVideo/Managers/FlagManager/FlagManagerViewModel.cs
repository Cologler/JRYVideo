using System.ComponentModel;
using JryVideo.Common;
using JryVideo.Model;

namespace JryVideo.Managers.FlagManager
{
    public class FlagManagerViewModel : JasilyViewModel
    {
         
    }

    public class FlagEditableViewModel : FlagViewModel
    {
        public FlagEditableViewModel(JryFlag source)
            : base(source)
        {
        }
    }
}