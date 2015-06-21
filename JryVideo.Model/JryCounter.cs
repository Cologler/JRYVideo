using System;

namespace JryVideo.Model
{
    public class JryCounter : JryObject
    {
        public JryCounterType Type { get; set; }

        public string Value { get; set; }

        public int Count { get; set; }

        /// <summary>
        /// Guid.NewGuid().ToString().ToUpper()
        /// </summary>
        /// <returns></returns>
        protected override string BuildId()
        {
            return BuildCounterId(this.Type, this.Value);
        }

        public static string BuildCounterId(JryCounterType type, string value)
        {
            return String.Format("{0}/{1}", (int)type, value.ThrowIfNullOrEmpty("value"));
        }
    }
}