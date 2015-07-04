using System;
using System.Collections.Generic;
using System.ComponentModel.Editable;
using System.Diagnostics;

namespace JryVideo.Model
{
    public class JryFlag : JryObject, IJasilyLoggerObject<JryFlag>
    {
        public JryFlagType Type { get; set; }

        [EditableField]
        public string Value { get; set; }

        public int Count { get; set; }

        protected override string BuildId()
        {
            return BuildCounterId(this.Type, this.Value);
        }

        public static string BuildCounterId(JryFlagType type, string value)
        {
            return String.Format("{0}/{1}", (int)type, value.ThrowIfNullOrEmpty("value"));
        }

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

        public static bool IsValueInvalid(string value)
        {
            return value.IsNullOrWhiteSpace();
        }
    }
}