using System;

namespace JryVideo.Data
{
    public struct SearchElement
    {
        public readonly ElementType Type;
        public readonly string Value;

        public SearchElement(ElementType type, string value)
        {
            this.Type = type;
            this.Value = value.ThrowIfNullOrEmpty("value");
        }

        public enum ElementType
        {
            Text,
            SeriesId,
            VideoId,
            EntityId,
            DoubanId
        }
    }
}