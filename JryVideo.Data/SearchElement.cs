using System;

namespace JryVideo.Data
{
    public struct SearchElement
    {
        private readonly ElementType type;
        private readonly string value;

        public SearchElement(ElementType type, string value)
        {
            this.type = type;
            this.value = value.ThrowIfNullOrEmpty("value");
        }

        public ElementType Type
        {
            get { return this.type; }
        }

        public string Value
        {
            get { return this.value; }
        }

        public enum ElementType
        {
            Text,
            Id
        }
    }
}