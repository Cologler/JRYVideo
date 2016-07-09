using System;
using System.Collections.Generic;
using System.Net;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

namespace JryVideo.Core.Models
{
    public class CoverBuilder
    {
        public CoverBuilder(ICoverParent coverParent)
        {
            this.Id = coverParent.Id;
            if (this.Id == null) throw new ArgumentNullException();
        }

        public CoverType CoverType { get; set; }

        public JryCoverSourceType CoverSourceType { get; set; }

        public List<HttpWebRequest> Requests { get; } = new List<HttpWebRequest>();

        public List<string> Uri { get; } = new List<string>();

        public string DoubanId { get; set; }

        public string ImdbId { get; set; }

        public string VideoId { get; set; }

        public string SeriesId { get; set; }

        public string ActorId { get; set; }

        /// <summary>
        /// 自定义的 Cover Id
        /// </summary>
        public string Id { get; }

        public JryCover Build(byte[] binaryData)
        {
            var cover = new JryCover()
            {
                CoverType = this.CoverType,
                CoverSourceType = this.CoverSourceType,

                DoubanId = this.DoubanId,
                ImdbId = this.ImdbId,

                VideoId = this.VideoId,
                SeriesId = this.SeriesId,
                ActorId = this.ActorId,

                BinaryData = binaryData
            };
            cover.BuildMetaData();
            cover.Id = this.Id;
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
            return new CoverBuilder(video)
            {
                CoverSourceType = JryCoverSourceType.Douban,
                CoverType = CoverType.Video,
                DoubanId = video.DoubanId,
                ImdbId = video.ImdbId,
                VideoId = video.Id
            };
        }

        public static CoverBuilder CreateBackground(JryVideoInfo video, string url)
        {
            return new CoverBuilder(video.BackgroundImageAsCoverParent())
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = CoverType.Background,
                DoubanId = video.DoubanId,
                Uri =
                {
                    url
                },
                ImdbId = video.ImdbId,
                VideoId = video.Id
            };
        }

        public static CoverBuilder CreateRole(JrySeries series, string url, VideoRole role)
        {
            return new CoverBuilder(role)
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = CoverType.Role,
                Uri =
                {
                    url
                },
                SeriesId = series.Id,
                ActorId = role.ActorId
            };
        }

        public static CoverBuilder CreateRole(JryVideoInfo video, string url, VideoRole role)
        {
            return new CoverBuilder(role)
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = CoverType.Role,
                Uri =
                {
                    url
                },
                VideoId = video.Id,
                ActorId = role.ActorId
            };
        }

        public static CoverBuilder CreateArtist(string doubanId, string url, Artist artist)
        {
            return new CoverBuilder(artist)
            {
                CoverSourceType = JryCoverSourceType.Douban,
                CoverType = CoverType.Artist,
                DoubanId = doubanId,
                Uri =
                {
                    url
                },
                ActorId = artist.Id
            };
        }
    }
}