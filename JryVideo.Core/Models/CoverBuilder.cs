using JryVideo.Model;
using System;

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

        public string BuildDownloadId()
        {
            var key = ((int)this.CoverType) + "_";

            switch (this.CoverType)
            {
                case JryCoverType.Role:
                    return key + (this.VideoId ?? this.SeriesId) + "_" + this.ActorId;

                case JryCoverType.Background:
                case JryCoverType.Video:
                    return key + this.VideoId;

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
            return cover;
        }

        public JryCover.QueryParameter BuildQueryParameter()
        {
            var queryParameter = new JryCover.QueryParameter()
            {
                CoverType = this.CoverType
            };

            switch (this.CoverType)
            {
                case JryCoverType.Artist:
                    break;

                case JryCoverType.Video:
                case JryCoverType.Background:
                    queryParameter.VideoId = this.VideoId;
                    return queryParameter;

                case JryCoverType.Role:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new ArgumentOutOfRangeException();
        }

        public static CoverBuilder CreateVideo(JryVideoInfo video, string url)
        {
            var cover = new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Douban,
                CoverType = JryCoverType.Video,
                DoubanId = video.DoubanId,
                Uri = url,
                ImdbId = video.ImdbId,
                VideoId = video.Id
            };
            return cover;
        }

        public static CoverBuilder CreateBackground(JryVideoInfo video, string url)
        {
            var cover = new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = JryCoverType.Background,
                DoubanId = video.DoubanId,
                Uri = url,
                ImdbId = video.ImdbId,
                VideoId = video.Id
            };
            return cover;
        }

        public static CoverBuilder CreateRole(JrySeries series, string url, JryVideoRole role)
        {
            var cover = new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = JryCoverType.Role,
                Uri = url,
                SeriesId = series.Id,
                ActorId = role.Id
            };
            return cover;
        }

        public static CoverBuilder CreateRole(JryVideoInfo video, string url, JryVideoRole role)
        {
            var cover = new CoverBuilder
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = JryCoverType.Role,
                Uri = url,
                VideoId = video.Id,
                ActorId = role.Id
            };
            return cover;
        }
    }
}