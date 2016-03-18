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

        public List<MapperValue> ExtendSubTitleLanguages { get; set; }

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
            => await Task.Run(() => this.TryFire(this.GetSource(type), name, filter));

        public string[] TryFire(IEnumerable<MapperValue> mapper, string name, Func<string, bool> filter = null)
        {
            var source = mapper?
                    .Where(z => z.From != null && z.To != null)
                    .Where(item => item.From.Where(z => z.Length > 0).FirstOrDefault(z => name.Contains(z, StringComparison.OrdinalIgnoreCase)) != null)
                    .SelectMany(item => item.To)
                    .Distinct();
            if (source == null) return new string[0];
            if (filter != null)
            {
                source = source.Where(filter);
            }
            return source.ToArray();
        }
    }
}