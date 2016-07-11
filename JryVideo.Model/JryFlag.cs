using System;
using System.Diagnostics;
using JryVideo.Model.Interfaces;

namespace JryVideo.Model
{
    public class JryFlag : RootObject, IJasilyLoggerObject<JryFlag>, IUpdated
    {
        public JryFlagType Type { get; set; }

        public string Value { get; set; }

        public int Count { get; set; }

        protected override void BuildId() => this.Id = BuildFlagId(this.Type, this.Value);

        public static string BuildFlagId(JryFlagType type, string value)
            => $"{(int)type}/{value.ThrowIfNullOrEmpty("value")}";

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.Count < 0)
            {
                this.Log(JasilyLogger.LoggerMode.Debug, "flag count can not less than 0.");
                return true;
            }

            if (IsValueInvalid(this.Value))
            {
                this.Log(JasilyLogger.LoggerMode.Release, "flag value can not be empty.");
                return true;
            }

            return false;
        }

        public static bool IsValueInvalid(string value) => value.IsNullOrWhiteSpace();

        public DateTime Updated { get; set; }

        public override void CheckError()
        {
            base.CheckError();
            DataChecker.True(this.Count >= 0);
            DataChecker.NotEmpty(this.Value);
        }
    }
}