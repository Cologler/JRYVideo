using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JryVideo.Core.TheTVDB
{
    public class Banner
    {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("BannerPath")]
        public string BannerPath { get; set; }

        public async Task<byte[]> GetBannerAsync(TheTVDBClient client)
            => await client.GetBannerByBannerPathAsync(this.BannerPath);

        public string BuildUrl(TheTVDBClient client) => client.BuildBannerUrl(this.BannerPath);

        [XmlElement("BannerType")]
        public BannerType BannerType { get; set; }

        [XmlElement("BannerType2")]
        public string Resolution { get; set; }

        [XmlElement("Colors")]
        public string Colors { get; set; }

        [XmlElement("Language")]
        public string Language { get; set; }

        [XmlElement("Rating")]
        public string Rating { get; set; }

        [XmlElement("RatingCount")]
        public string RatingCount { get; set; }

        [XmlElement("Season")]
        public string Season { get; set; }

        [XmlElement("SeriesName")]
        public string SeriesName { get; set; }

        [XmlElement("ThumbnailPath")]
        public string ThumbnailPath { get; set; }

        [XmlElement("VignettePath")]
        public string VignettePath { get; set; }
    }
}