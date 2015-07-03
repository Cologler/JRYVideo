using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JryVideo.Model
{
    public class JryFlag : JryObject
    {
        public JryFlagType Type { get; set; }

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

            if (this.Count < 1)
            {
                JasilyLogger.Current.WriteLine<JryFlag>(JasilyLogger.LoggerMode.Debug, "flag count can not less than 1.");
                return true;
            }

            if (!IsValueValid(this.Value))
            {
                JasilyLogger.Current.WriteLine<JryFlag>(JasilyLogger.LoggerMode.Release, "flag value can not be empty.");
                return true;
            }

            return false;
        }

        public static bool IsValueValid(string value)
        {
            return !value.IsNullOrWhiteSpace();
        }
    }
}