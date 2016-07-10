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
            this.Id = coverParent.CoverId;
            if (this.Id == null) throw new ArgumentNullException();
        }

        public CoverType CoverType { get; set; }

        public List<HttpWebRequest> Requests { get; } = new List<HttpWebRequest>();

        public List<string> Uri { get; } = new List<string>();

        /// <summary>
        /// 自定义的 Cover Id
        /// </summary>
        public string Id { get; }

        public JryCover Build(byte[] binaryData)
        {
            var cover = new JryCover()
            {
                CoverType = this.CoverType,
                BinaryData = binaryData
            };
            cover.BuildMetaData();
            cover.Id = this.Id;
            return cover;
        }

        public static CoverBuilder CreateVideo(JryVideoInfo video)
        {
            return new CoverBuilder(video)
            {
                CoverType = CoverType.Video,
            };
        }

        public static CoverBuilder CreateBackground(JryVideoInfo video)
        {
            return new CoverBuilder(video.BackgroundImageAsCoverParent())
            {
                CoverType = CoverType.Background
            };
        }

        public static CoverBuilder CreateRole(VideoRole role)
        {
            return new CoverBuilder(role)
            {
                CoverType = CoverType.Role
            };
        }

        public static CoverBuilder CreateArtist(Artist artist)
        {
            return new CoverBuilder(artist)
            {
                CoverType = CoverType.Artist
            };
        }
    }
}