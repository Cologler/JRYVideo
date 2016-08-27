using System;
using System.Diagnostics;
using JryVideo.Model.Interfaces;

namespace JryVideo.Model
{
    public class JryFlag : RootObject, IJasilyLoggerObject<JryFlag>, IUpdated,
        IQueryBy<JryFlagType>
    {
        public JryFlagType Type { get; set; }

        public string Value { get; set; }

        public int Count { get; set; }

        protected override void BuildId() => this.Id = BuildFlagId(this.Type, this.Value);

        public static string BuildFlagId(JryFlagType type, string value)
            => $"{(int)type}/{value.ThrowIfNullOrEmpty("value")}";

        public static bool IsValueInvalid(string value) => value.IsNullOrWhiteSpace();

        public DateTime Updated { get; set; }

        public override void CheckError()
        {
            base.CheckError();
            DataCheck.True(this.Count >= 0);
            DataCheck.False(IsValueInvalid(this.Value));
        }
    }
}