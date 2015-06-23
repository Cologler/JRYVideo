using System;
using System.Collections.Generic;

namespace JryVideo.Model
{
    public class JryCounter : JryObject
    {
        public JryCounter(JryCounterType type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        public JryCounterType Type { get; set; }

        public string Value { get; set; }

        public int Count { get; set; }

        protected override string BuildId()
        {
            return BuildCounterId(this.Type, this.Value);
        }

        public static string BuildCounterId(JryCounterType type, string value)
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

            if (String.IsNullOrWhiteSpace(this.Value))
            {
                yield return JryInvalidError.NameCanNotBeEmpty;
            }
        }
    }
}