using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Core.Managers.Upgrades.Patchs
{
    public class Patch0003 : IPatch<Series>
    {
        public Task<bool> UpgradeAsync(Series series)
        {
            foreach (var video in series.Videos)
            {
#pragma warning disable 612
                video.Version = 0;
#pragma warning restore 612
            }

            return Patch0000.TrueTask;
        }
    }
}