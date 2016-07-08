using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Core.Managers.Upgrades
{
    public class UpgraderInfoV1 : IUpgraderInfo
    {
        private readonly DataCenter dataCenter;

        public UpgraderInfoV1(DataCenter dataCenter)
        {
            this.dataCenter = dataCenter;
        }

#pragma warning disable CS0612 // 类型或成员已过时

        public async Task<bool> UpgradeAsync(JrySeries series)
        {
            foreach (var video in series.Videos)
            {
                if (video.CoverId != null && video.CoverId != video.Id)
                {
                    var cover = await this.dataCenter.CoverManager.Source.FindAsync(video.CoverId);
                    if (cover != null)
                    {
                        cover.Id = video.Id;
                        await this.dataCenter.CoverManager.Source.InsertOrUpdateAsync(cover);
                        video.CoverId = video.Id;
                    }
                }

                if (video.BackgroundImageId != null)
                {
                    var cover = await this.dataCenter.CoverManager.Source.FindAsync(video.BackgroundImageId);
                    if (cover != null)
                    {
                        var newId = video.Id + ":Background";
                        cover.Id = newId;
                        await this.dataCenter.CoverManager.Source.InsertOrUpdateAsync(cover);
                        video.BackgroundImageId = newId;
                    }
                }
            }

            return true;
        }

        public async Task<bool> UpgradeAsync(JryCover cover)
        {
            if (cover.CoverType == JryCoverType.Video)
            {
                if (cover.VideoId != null && cover.Id != cover.VideoId)
                {
                    var oldCoverId = cover.Id;
                    cover.Id = cover.VideoId;
                    await this.dataCenter.CoverManager.Source.InsertOrUpdateAsync(cover);
                    await this.dataCenter.CoverManager.Source.RemoveAsync(oldCoverId);
                }
            }

            if (cover.CoverType == JryCoverType.Background)
            {
                if (cover.VideoId != null && cover.Id != cover.VideoId + ":Background")
                {
                    var oldCoverId = cover.Id;
                    cover.Id = cover.VideoId + ":Background";
                    await this.dataCenter.CoverManager.Source.InsertOrUpdateAsync(cover);
                    await this.dataCenter.CoverManager.Source.RemoveAsync(oldCoverId);
                }
            }

            // return true to update old item, but we remove old item.
            return false;
        }

#pragma warning restore CS0612 // 类型或成员已过时
    }

    public class UpgraderInfoV2 : IUpgraderInfo
    {
        public Task<bool> UpgradeAsync(JrySeries series)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> UpgradeAsync(JryCover cover)
        {
            return Task.FromResult(false);
        }
    }
}