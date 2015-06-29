using JryVideo.Common;
using JryVideo.Model;

namespace JryVideo.Controls.EditFlag
{
    public class EditFlagViewModel : EditorItemViewModel<JryFlag>
    {
        private string value;

        public string Value
        {
            get { return this.value; }
            set { this.SetPropertyRef(ref this.value, value); }
        }
    }
}