using System.Enums;
using System.Threading.Tasks;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Model;

namespace JryVideo.Controls.EditFlag
{
    public class EditFlagViewModel : EditorItemViewModel<JryFlag>
    {
        private string value;

        public string Value
        {
            get { return this.value ?? ""; }
            set { this.SetPropertyRef(ref this.value, value); }
        }

        public async Task<JryFlag> CommitAsync(JryFlagType type)
        {
            var manager = JryVideoCore.Current.CurrentDataCenter.FlagManager;

            var obj = this.GetCommitObject();
            obj.Type = type;
            this.WriteToObject(obj);

            if (this.Action == ObjectChangedAction.Create)
                obj.BuildMetaData();

            return await base.CommitAsync(manager, obj);
        }
    }
}