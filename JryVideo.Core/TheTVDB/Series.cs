using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JryVideo.Core.TheTVDB
{
    public class Series
    {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("seriesid")]
        public string SeriesId { get; set; }

        [XmlElement("language")]
        public string Language { get; set; }

        [XmlElement("SeriesName")]
        public string SeriesName { get; set; }

        [XmlElement("banner")]
        public string Banner { get; set; }

        [XmlElement("Overview")]
        public string Overview { get; set; }

        [XmlIgnore]
        public DateTime FirstAired => DateTime.Parse(this.FirstAiredString);

        [XmlElement("FirstAired")]
        public string FirstAiredString { get; set; }

        [XmlElement("IMDB_ID")]
        public string ImdbId { get; set; }

        [XmlElement("zap2it_id")]
        public string Zap2ItId { get; set; }

        public async Task<byte[]> GetBannerAsync(TheTVDBClient client)
            => await client.GetBannerByBannerPathAsync(this.Banner);

        public async Task<IEnumerable<Banner>> GetBannersAsync(TheTVDBClient client)
            => await client.GetBannersBySeriesIdAsync(this.Id);

        public async Task<IEnumerable<Actor>> GetActorsAsync(TheTVDBClient client)
            => await client.GetActorsBySeriesIdAsync(this.Id);
    }
}