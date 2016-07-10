using System.Diagnostics;
using System.Threading.Tasks;
using JryVideo.Model;

#pragma warning disable 612

namespace JryVideo.Core.Managers.Upgrades.Patchs
{
    public class Patch0002 : IPatch<JrySeries>, IPatch<JryCover>
    {
        public Task<bool> UpgradeAsync(JrySeries series)
        {
            foreach (var video in series.Videos)
            {
                if (video.CoverId == null) continue;
                Debug.Assert(video.CoverId == video.Id);
                video.CoverId = null;
            }

            return Task.FromResult(true);
        }

        public Task<bool> UpgradeAsync(JryCover item)
        {
            item.CoverSourceType = default(JryCoverSourceType);
            item.DoubanId = null;
            item.ImdbId = null;
            item.SeriesId = null;
            item.VideoId = null;
            item.ActorId = null;
            item.Uri = null;
            return Patch0000.TrueTask;
        }
    }
}