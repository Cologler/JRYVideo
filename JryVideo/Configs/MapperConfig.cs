using System;
using System.Collections.Generic;
using JryVideo.Model;

namespace JryVideo.Configs
{
    public sealed class MapperConfig
    {
        public List<MapperValue> Tags { get; set; }

        public List<MapperValue> Fansubs { get; set; }

        public List<MapperValue> SubTitleLanguages { get; set; }

        public List<MapperValue> TrackLanguages { get; set; }

        public List<MapperValue> ExtendSubTitleLanguages { get; set; }

        public IEnumerable<MapperValue> GetByFlagType(JryFlagType type)
        {
            switch (type)
            {
                case JryFlagType.ResourceFansub:
                    return this.Fansubs ?? Empty<MapperValue>.Enumerable;

                case JryFlagType.ResourceSubTitleLanguage:
                    return this.SubTitleLanguages ?? Empty<MapperValue>.Enumerable;

                case JryFlagType.ResourceTrackLanguage:
                    return this.TrackLanguages ?? Empty<MapperValue>.Enumerable;

                case JryFlagType.ResourceTag:
                    return this.Tags ?? Empty<MapperValue>.Enumerable;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}