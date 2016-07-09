using System;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Model;
#pragma warning disable 612

namespace JryVideo.Core.Managers.Upgrades.Patchs
{
    public class Patch0001 : IGlobalPatch<JrySeries>, IGlobalPatch<JryCover>, IGlobalPatch<VideoRoleCollection>
    {
        public async Task<bool> UpgradeAsync(DataCenter dataCenter, JrySeries series)
        {
            foreach (var video in series.Videos)
            {
                if (video.CoverId != null && video.CoverId != video.Id)
                {
                    var cover = await dataCenter.CoverManager.Source.FindAsync(video.CoverId);
                    if (cover != null)
                    {
                        cover.Id = video.Id;
                        await dataCenter.CoverManager.Source.InsertOrUpdateAsync(cover);
                        video.CoverId = video.Id;
                    }
                }

                if (video.BackgroundImageId != null)
                {
                    var cover = await dataCenter.CoverManager.Source.FindAsync(video.BackgroundImageId);
                    if (cover != null)
                    {
                        var newId = video.CreateBackgroundCoverId();
                        cover.Id = newId;
                        await dataCenter.CoverManager.Source.InsertOrUpdateAsync(cover);
                        video.BackgroundImageId = newId;
                    }
                }
            }

            return true;
        }

        public async Task<bool> UpgradeAsync(DataCenter dataCenter, JryCover cover)
        {
            if (cover.CoverType == CoverType.Video)
            {
                if (cover.VideoId != null && cover.Id != cover.VideoId)
                {
                    var oldCoverId = cover.Id;
                    cover.Id = cover.VideoId;
                    await dataCenter.CoverManager.Source.InsertOrUpdateAsync(cover);
                    await dataCenter.CoverManager.Source.RemoveAsync(oldCoverId);
                }
            }

            if (cover.CoverType == CoverType.Background)
            {
                if (cover.VideoId != null && cover.Id != JryVideoInfo.CreateBackgroundCoverId(cover.VideoId))
                {
                    var oldCoverId = cover.Id;
                    cover.Id = JryVideoInfo.CreateBackgroundCoverId(cover.VideoId);
                    await dataCenter.CoverManager.Source.InsertOrUpdateAsync(cover);
                    await dataCenter.CoverManager.Source.RemoveAsync(oldCoverId);
                }
            }

            // return true to update old item, but we remove old item.
            return false;
        }

        /// <summary>
        /// return true if upgrade success
        /// </summary>
        /// <param name="dataCenter"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<bool> UpgradeAsync(DataCenter dataCenter, VideoRoleCollection item)
        {
            foreach (var role in (item.MajorRoles ?? Empty<VideoRole>.Enumerable).Concat(item.MinorRoles ?? Empty<VideoRole>.Enumerable))
            {
                if (role.ActorId == null)
                {
                    role.ActorId = role.Id;
                    role.Id = VideoRole.NewGuid();
                    if (role.CoverId != null)
                    {
                        var cover = await dataCenter.CoverManager.Source.FindAsync(role.CoverId);
                        cover.Id = role.Id;
                        await dataCenter.CoverManager.Source.InsertOrUpdateAsync(cover);
                        await dataCenter.CoverManager.Source.RemoveAsync(role.CoverId);
                        role.CoverId = null;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// return true if upgrade success
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> IPatch<JrySeries>.UpgradeAsync(JrySeries item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// return true if upgrade success
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> IPatch<JryCover>.UpgradeAsync(JryCover item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// return true if upgrade success
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> IPatch<VideoRoleCollection>.UpgradeAsync(VideoRoleCollection item)
        {
            throw new NotImplementedException();
        }
    }
}