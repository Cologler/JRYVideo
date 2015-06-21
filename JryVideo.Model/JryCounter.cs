using System;

namespace JryVideo.Model
{
    public class JryCounter : JryObject, IInitializable<JryCounter>
    {
        public JryCounterType Type { get; set; }

        public string Value { get; set; }

        public int Count { get; set; }

        public JryCounter InitializeInstance(JryCounter obj)
        {
            return base.InitializeInstance(obj);
        }

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