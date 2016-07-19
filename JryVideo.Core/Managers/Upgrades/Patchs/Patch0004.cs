using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Core.Managers.Upgrades.Patchs
{
    public class Patch0004 : IPatch<Series>
    {
        /// <summary>
        /// return true if upgrade success
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Task<bool> UpgradeAsync(Series item)
        {
            foreach (var video in item.Videos)
            {
#pragma warning disable 612
                video.BackgroundImageId = null;
#pragma warning restore 612
            }
            return Patch0000.TrueTask;
        }
    }
}