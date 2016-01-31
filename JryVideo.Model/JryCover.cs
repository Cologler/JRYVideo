﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Diagnostics;

namespace JryVideo.Model
{
    public sealed class JryCover : JryObject,
        JryCover.ISeriesRoleCover,
        JryCover.IVideoRoleCover
    {
        public JryCoverType CoverType { get; set; }

        public JryCoverSourceType CoverSourceType { get; set; }

        [BsonIgnoreIfDefault]
        public string DoubanId { get; set; }

        [BsonIgnoreIfDefault]
        public string ImdbId { get; set; }

        [BsonIgnoreIfDefault]
        public string VideoId { get; set; }

        [BsonIgnoreIfDefault]
        public string SeriesId { get; set; }

        [BsonIgnoreIfDefault]
        public string ActorId { get; set; }

        [BsonIgnoreIfDefault]
        public string Uri { get; set; }

        [BsonIgnoreIfDefault]
        public byte[] BinaryData { get; set; }

        public string GetDownloadId()
        {
            var key = ((int)this.CoverType) + "_";

            if (this.CoverType == JryCoverType.Role)
            {
                return ((int)this.CoverType) + "_" + (this.VideoId ?? this.SeriesId) + "_" + this.ActorId;
            }

            switch (this.CoverSourceType)
            {
                case JryCoverSourceType.Local:
                    throw new ArgumentException();

                case JryCoverSourceType.Uri:
                    key += this.Uri;
                    break;

                case JryCoverSourceType.Douban:
                    key += this.DoubanId;
                    break;

                case JryCoverSourceType.Imdb:
                    key += this.ImdbId;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return key;
        }

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.BinaryData == null || this.BinaryData.Length == 0)
            {
                JasilyLogger.Current.WriteLine<JryCover>(JasilyLogger.LoggerMode.Debug, "cover data can not be empty.");
                return true;
            }

            return false;
        }

        public static JryCover CreateVideo(JryVideoInfo video, string url)
        {
            var cover = new JryCover
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

        public static JryCover CreateBackground(JryVideoInfo video, string url)
        {
            var cover = new JryCover
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

        public static JryCover CreateRole(JrySeries series, string url, JryVideoRole role)
        {
            var cover = new JryCover
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = JryCoverType.Role,
                Uri = url,
                SeriesId = series.Id,
                ActorId = role.Id
            };
            return cover;
        }

        public static JryCover CreateRole(JryVideoInfo video, string url, JryVideoRole role)
        {
            var cover = new JryCover
            {
                CoverSourceType = JryCoverSourceType.Imdb,
                CoverType = JryCoverType.Role,
                Uri = url,
                VideoId = video.Id,
                ActorId = role.Id
            };
            return cover;
        }

        public struct QueryParameter
        {
            public JryCoverType CoverType { get; set; }

            public string VideoId { get; set; }
        }

        public interface ICover
        {
            JryCoverType CoverType { get; set; }

            JryCoverSourceType CoverSourceType { get; set; }

            byte[] BinaryData { get; set; }
        }

        public interface IRoleCover : ICover
        {
            string ActorId { get; set; }
        }

        public interface IVideoRoleCover : IRoleCover
        {
            string VideoId { get; set; }
        }

        public interface ISeriesRoleCover : IRoleCover
        {
            string SeriesId { get; set; }
        }
    }
}