using Jasily.ComponentModel;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Core.Managers;
using JryVideo.Model;
using System.Threading.Tasks;

namespace JryVideo.Controls.EditFlag
{
    public class EditFlagViewModel : EditorItemViewModel<JryFlag>
    {
        private string value;

        [EditableField]
        public string Value
        {
            get { return this.value ?? ""; }
            set { this.SetPropertyRef(ref this.value, value); }
        }

        public string OldValue { get; set; }

        public override void ModifyMode(JryFlag source)
        {
            this.OldValue = source.Value;

            base.ModifyMode(source);
        }

        public async Task<JryFlag> CommitAsync(JryFlagType type)
        {
            var manager = this.GetManagers().FlagManager;

            var obj = new JryFlag(); // 不管是修改还是创建都是创建一个新的
            obj.Type = type;
            this.WriteToObject(obj);
            obj.BuildMetaData();

            return await base.CommitAsync(manager, obj);
        }

        protected override async Task<bool> OnUpdateAsync(IObjectEditProvider<JryFlag> provider, JryFlag obj)
            => await ((FlagManager)provider).ReplaceAsync(obj.Type, this.OldValue, obj.Value);
    }
}