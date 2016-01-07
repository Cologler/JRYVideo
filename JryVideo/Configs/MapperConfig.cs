using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Configs
{
    public sealed class MapperConfig
    {
        public List<MapperValue> Tags { get; set; }

        public List<MapperValue> Fansubs { get; set; }

        public List<MapperValue> SubTitleLanguages { get; set; }

        public List<MapperValue> TrackLanguages { get; set; }

        private IEnumerable<MapperValue> GetSource(JryFlagType type)
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

        public async Task<string[]> TryFireAsync(JryFlagType type, string name, Func<string, bool> filter = null)
        {
            return await Task.Run(() =>
            {
                var source = this.GetSource(type)?
                    .Where(z => z.From != null && z.To != null)
                    .Where(item => item.From.FirstOrDefault(name.Contains) != null)
                    .Select(item => item.To);
                if (source == null) return new string[0];
                if (filter != null)
                {
                    source = source.Where(filter);
                }
                return source.ToArray();
            });
        }
    }
}