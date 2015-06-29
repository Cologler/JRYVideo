using System;
using System.Collections.Generic;

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

        public override IEnumerable<JryInvalidError> CheckError()
        {
            foreach (var error in base.CheckError())
            {
                yield return error;
            }

            if (this.Count < 1)
            {
                yield return JryInvalidError.CounterCountLessThanOne;
            }

            if (!IsValueValid(this.Value))
            {
                yield return JryInvalidError.NameCanNotBeEmpty;
            }
        }

        public static bool IsValueValid(string value)
        {
            return !String.IsNullOrWhiteSpace(value);
        }
    }
}