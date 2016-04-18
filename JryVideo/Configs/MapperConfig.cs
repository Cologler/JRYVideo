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
                    return this.Fansubs;
                case JryFlagType.EntitySubTitleLanguage:
                    return this.SubTitleLanguages;
                case JryFlagType.EntityTrackLanguage:
                    return this.TrackLanguages;
                case JryFlagType.EntityTag:
                    return this.Tags;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}