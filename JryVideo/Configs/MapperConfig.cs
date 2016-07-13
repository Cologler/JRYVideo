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
                case JryFlagType.EntityFansub:
                    return this.Fansubs ?? Empty<MapperValue>.Enumerable;

                case JryFlagType.EntitySubTitleLanguage:
                    return this.SubTitleLanguages ?? Empty<MapperValue>.Enumerable;

                case JryFlagType.EntityTrackLanguage:
                    return this.TrackLanguages ?? Empty<MapperValue>.Enumerable;

                case JryFlagType.EntityTag:
                    return this.Tags ?? Empty<MapperValue>.Enumerable;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}