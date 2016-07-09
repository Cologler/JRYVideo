using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Model;
#pragma warning disable 612

namespace JryVideo.Core.Managers.Upgrades.Patchs
{
    public class Patch0001 : IGlobalPatch<JrySeries>, IGlobalPatch<JryCover>, IGlobalPatch<VideoRoleCollection>, IGlobalPatch<Artist>
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
                        await CoverRenamekeyAsync(dataCenter, role.CoverId, role.Id);
                        role.CoverId = null;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// return true if upgrade success
        /// </summary>
        /// <param name="dataCenter"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<bool> UpgradeAsync(DataCenter dataCenter, Artist item)
        {
            if (item.CoverId != null)
            {
                if (item.CoverId != item.Id)
                {
                    await CoverRenamekeyAsync(dataCenter, item.CoverId, item.Id);
                    item.CoverId = null;
                }
                item.CoverId = null;
            }
            return true;
        }

        private static async Task CoverRenamekeyAsync(DataCenter dataCenter, string oldKey, string newKey)
        {
            Debug.Assert(oldKey != null);
            var cover = await dataCenter.CoverManager.Source.FindAsync(oldKey);
            if (cover == null) return;
            cover.Id = newKey;
            await dataCenter.CoverManager.Source.InsertAsync(cover);
            await dataCenter.CoverManager.Source.RemoveAsync(oldKey);
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

        Task<bool> IPatch<Artist>.UpgradeAsync(Artist item)
        {
            throw new NotImplementedException();
        }
    }
}