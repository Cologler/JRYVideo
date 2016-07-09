using System;
using JryVideo.Model;

namespace JryVideo.Core.Models
{
    public class CoverBuilder
    {
        public CoverType CoverType { get; set; }

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
                case CoverType.Role:
                case CoverType.Background:
                case CoverType.Video:
                    return key + this.CustomId;

                case CoverType.Artist:
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
                CoverType = CoverType.Video,
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
                CoverType = CoverType.Background,
                DoubanId = video.DoubanId,
                Uri = url,
                ImdbId = video.ImdbId,
                VideoId = video.Id,
                CustomId = video.CreateBackgroundCoverId()
            };
        }

        public static CoverBuilder CreateRole(JrySeries series, string url, VideoRole role)
        {
            return new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = CoverType.Role,
                Uri = url,
                SeriesId = series.Id,
                ActorId = role.ActorId,
                CustomId = role.Id
            };
        }

        public static CoverBuilder CreateRole(JryVideoInfo video, string url, VideoRole role)
        {
            return new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = CoverType.Role,
                Uri = url,
                VideoId = video.Id,
                ActorId = role.ActorId,
                CustomId = role.Id
            };
        }

        public static CoverBuilder CreateArtist(string doubanId, string url, Artist artist)
        {
            return new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Douban,
                CoverType = CoverType.Artist,
                DoubanId = doubanId,
                Uri = url,
                ActorId = artist.Id
            };
        }
    }
}