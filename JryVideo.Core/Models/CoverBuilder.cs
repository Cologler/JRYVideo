using System;
using JryVideo.Model;

namespace JryVideo.Core.Models
{
    public class CoverBuilder
    {
        public JryCoverType CoverType { get; set; }

        public JryCoverSourceType CoverSourceType { get; set; }

        public string Uri { get; set; }

        public string DoubanId { get; set; }

        public string ImdbId { get; set; }

        public string VideoId { get; set; }

        public string SeriesId { get; set; }

        public string ActorId { get; set; }

        /// <summary>
        /// 自定义的 Cover Id
        /// </summary>
        public string CustomId { get; set; }

        public string BuildDownloadId()
        {
            var key = (int)this.CoverType + "_";

            switch (this.CoverType)
            {
                case JryCoverType.Role:
                    return key + (this.VideoId ?? this.SeriesId) + "_" + this.ActorId;

                case JryCoverType.Background:
                case JryCoverType.Video:
                    return key + this.VideoId;

                case JryCoverType.Artist:
                    return key + this.ActorId;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public JryCover Build(byte[] binaryData)
        {
            var cover = new JryCover()
            {
                CoverType = this.CoverType,
                CoverSourceType = this.CoverSourceType,

                Uri = this.Uri,

                DoubanId = this.DoubanId,
                ImdbId = this.ImdbId,

                VideoId = this.VideoId,
                SeriesId = this.SeriesId,
                ActorId = this.ActorId,

                BinaryData = binaryData
            };
            cover.BuildMetaData();
            cover.Id = this.CustomId ?? cover.Id;
            return cover;
        }

        public JryCover.QueryParameter BuildQueryParameter()
        {
            var queryParameter = new JryCover.QueryParameter()
            {
                CoverType = this.CoverType,
                VideoId = this.VideoId,
                SeriesId = this.SeriesId,
                ActorId = this.ActorId
            };

            return queryParameter;
        }

        public static CoverBuilder CreateVideo(JryVideoInfo video)
        {
            return new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Douban,
                CoverType = JryCoverType.Video,
                DoubanId = video.DoubanId,
                ImdbId = video.ImdbId,
                VideoId = video.Id,
                CustomId = video.Id
            };
        }

        public static CoverBuilder CreateBackground(JryVideoInfo video, string url)
        {
            return new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = JryCoverType.Background,
                DoubanId = video.DoubanId,
                Uri = url,
                ImdbId = video.ImdbId,
                VideoId = video.Id,
                CustomId = video.CreateBackgroundCoverId()
            };
        }

        public static CoverBuilder CreateRole(JrySeries series, string url, JryVideoRole role)
        {
            return new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = JryCoverType.Role,
                Uri = url,
                SeriesId = series.Id,
                ActorId = role.Id
            };
        }

        public static CoverBuilder CreateRole(JryVideoInfo video, string url, JryVideoRole role)
        {
            return new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = JryCoverType.Role,
                Uri = url,
                VideoId = video.Id,
                ActorId = role.Id
            };
        }

        public static CoverBuilder CreateArtist(string doubanId, string url, Artist artist)
        {
            return new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Douban,
                CoverType = JryCoverType.Artist,
                DoubanId = doubanId,
                Uri = url,
                ActorId = artist.Id
            };
        }
    }
}