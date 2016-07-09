using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Core.Managers.Upgrades
{
    public class Patch0001 : IPatch<JrySeries>, IPatch<JryCover>
    {
        private readonly DataCenter dataCenter;

        public Patch0001(DataCenter dataCenter)
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
                        var newId = video.CreateBackgroundCoverId();
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
            if (cover.CoverType == CoverType.Video)
            {
                if (cover.VideoId != null && cover.Id != cover.VideoId)
                {
                    var oldCoverId = cover.Id;
                    cover.Id = cover.VideoId;
                    await this.dataCenter.CoverManager.Source.InsertOrUpdateAsync(cover);
                    await this.dataCenter.CoverManager.Source.RemoveAsync(oldCoverId);
                }
            }

            if (cover.CoverType == CoverType.Background)
            {
                if (cover.VideoId != null && cover.Id != JryVideoInfo.CreateBackgroundCoverId(cover.VideoId))
                {
                    var oldCoverId = cover.Id;
                    cover.Id = JryVideoInfo.CreateBackgroundCoverId(cover.VideoId);
                    await this.dataCenter.CoverManager.Source.InsertOrUpdateAsync(cover);
                    await this.dataCenter.CoverManager.Source.RemoveAsync(oldCoverId);
                }
            }

            // return true to update old item, but we remove old item.
            return false;
        }

#pragma warning restore CS0612 // 类型或成员已过时
    }
}