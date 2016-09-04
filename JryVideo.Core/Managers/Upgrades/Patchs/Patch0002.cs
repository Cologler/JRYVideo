using System.Diagnostics;
using System.Threading.Tasks;
using JryVideo.Model;

#pragma warning disable 612

namespace JryVideo.Core.Managers.Upgrades.Patchs
{
    public class Patch0002 : IPatch<Series>, IPatch<JryCover>
    {
        public Task<bool> UpgradeAsync(Series series)
        {
            foreach (var video in series.Videos)
            {
                if (video.CoverId == null) continue;
                Debug.Assert(video.CoverId == video.Id);
                video.CoverId = null;
            }

            return Patch0000.TrueTask;
        }

        public Task<bool> UpgradeAsync(JryCover item)
        {
            item.CoverSourceType = default(int);
            item.DoubanId = null;
            item.ImdbId = null;
            item.SeriesId = null;
            item.VideoId = null;
            item.ActorId = null;
            item.Uri = null;
            return Patch0000.TrueTask;
        }

        public Task<bool> UpgradeAsync(Model.JryVideo item)
        {
            throw new System.NotImplementedException();
        }
    }
}