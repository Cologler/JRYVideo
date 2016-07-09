using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Core.Managers.Upgrades
{
    public class Patch0003 : IPatch<JrySeries>
    {
        public Task<bool> UpgradeAsync(JrySeries series)
        {
            foreach (var video in series.Videos)
            {
#pragma warning disable 612
                video.Version = 0;
#pragma warning restore 612
            }

            return Task.FromResult(true);
        }
    }
}