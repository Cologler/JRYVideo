using System;
using System.Threading.Tasks;
using Jasily.ComponentModel.Editable;
using JryVideo.Common;
using JryVideo.Core.Managers;
using JryVideo.Model;

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

        public JryFlagType? FlagType { get; set; }

        public override void ModifyMode(JryFlag source)
        {
            this.OldValue = source.Value;
            base.ModifyMode(source);
        }

        protected override JryFlag NewInstance() => new JryFlag { Value = "_" };

        public async Task<JryFlag> CommitAsync()
        {
            var flagType = this.FlagType;
            if (!flagType.HasValue) throw new Exception();
            // 不管是修改还是创建都是创建一个新的
            var obj = new JryFlag();
            obj.Type = flagType.Value;
            this.WriteToObject(obj);
            obj.BuildMetaData();
            return await base.CommitAsync(this.GetManagers().FlagManager, obj);
        }

        protected override async Task<bool> OnUpdateAsync(IObjectEditProvider<JryFlag> provider, JryFlag obj)
            => await ((FlagManager)provider).ReplaceAsync(obj.Type, this.OldValue, obj.Value);
    }
}