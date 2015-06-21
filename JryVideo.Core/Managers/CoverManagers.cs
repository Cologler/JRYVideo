using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using JryVideo.Core.Douban;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class CoverManager
    {
        private readonly object _syncRoot = new object();
        private readonly List<string> _writingDoubanId = new List<string>(); 

        public ICoverDataSourceProvider Source { get; private set; }

        public CoverManager(ICoverDataSourceProvider source)
        {
            this.Source = source;
        }

        public async Task<JryCover> LoadCoverAsync(string coverId)
        {
            return coverId == null ? null : await this.Source.QueryAsync(coverId);
        }

        public async Task<string> UpdateCoverFromDoubanIdAsync(string doubanId)
        {
            if (String.IsNullOrWhiteSpace(doubanId))
                throw new ArgumentException();

            return await Task.Run<string>(async () =>
            {
                foreach (var jryCover in await this.Source.QueryByDoubanIdAsync(JryCoverType.Video, doubanId))
                {
                    return jryCover.Id;
                }

                lock (this._syncRoot)
                {
                    if (this._writingDoubanId.Contains(doubanId)) return null;

                    this._writingDoubanId.Add(doubanId);
                }

                var json = await DoubanHelper.GetMovieInfoAsync(doubanId);

                if (json == null || json.Images == null || json.Images.Large == null)
                {
                    return null;
                }

                var url = DoubanHelper.GetLargeImageUrl(json);

                var request = WebRequest.CreateHttp(url);

                var result = await request.GetResultAsBytesAsync();

                if (result.IsSuccess)
                {
                    var cover = new JryCover().InitializeInstance();
                    cover.CoverSourceType = JryCoverSourceType.Douban;
                    cover.CoverType = JryCoverType.Video;
                    cover.DoubanId = doubanId;
                    cover.Uri = json.Images.Large;
                    cover.BinaryData = result.Result;
                    await this.InsertAsync(cover);

                    lock (this._syncRoot)
                    {
                        this._writingDoubanId.Remove(doubanId);
                    }

                    return cover.Id;
                }
                else
                {
                    lock (this._syncRoot)
                    {
                        this._writingDoubanId.Remove(doubanId);
                    }

                    return null;
                }
            });
        }

        public async Task InsertAsync(JryCover cover)
        {
            await this.Source.InsertAsync(cover);
        }

        public async Task UpdateAsync(JryCover cover)
        {
            await this.Source.UpdateAsync(cover);
        }

        //public Video5Cover this[string doubanId]
        //{
        //    get
        //    {
        //        if (doubanId == null)
        //            return null;
        //        return Collection<>.FindOne(Query<Video5Cover>.EQ(e => e.DoubanId, doubanId));
        //    }
        //}

        //public byte[] this[Video5Entity entity]
        //{
        //    get
        //    {
        //        var cover = this[entity.DoubanId];

        //        if (cover != null)
        //            return cover.CoverData;
        //        else
        //            return null;
        //    }
        //    set
        //    {
        //        if (value == null || entity.DoubanId == null)
        //            return;

        //        var cover = this[entity.DoubanId];

        //        if (cover == null)
        //            cover = Video5Cover.Create(entity.DoubanId, value);

        //        Collection.Save(cover);
        //    }
        //}
    }
}