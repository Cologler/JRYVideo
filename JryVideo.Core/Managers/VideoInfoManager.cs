using JryVideo.Model;
using System;
using System.Data;
using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public class VideoInfoManager : JryObjectManager<JryVideoInfo, IJasilyEntitySetProvider<JryVideoInfo, string>>
    {
        public VideoInfoManager(IJasilyEntitySetProvider<JryVideoInfo, string> source)
            : base(source)
        {
        }

        public override async Task<bool> RemoveAsync(string id)
        {
            var item = await this.FindAsync(id);
            if (item != null)
            {
                this.OnCoverParentRemoving(item.BackgroundImageAsCoverParent());
                this.OnCoverParentRemoving(item);
            }
            return await base.RemoveAsync(id);
        }

        internal override Task<CombineResult> CanCombineAsync(JryVideoInfo to, JryVideoInfo from)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (from == null) throw new ArgumentNullException(nameof(from));

            return Task.FromResult(this.CanCombine(to, from));
        }

        private CombineResult CanCombine(JryVideoInfo to, JryVideoInfo from)
        {
            if (to.Type != from.Type)
            {
                return CombineResult.False("have diff type.");
            }

            if (to.Year != from.Year)
            {
                return CombineResult.False("have diff year.");
            }

            if (to.Index != from.Index)
            {
                return CombineResult.False("have diff index.");
            }

            if (to.LastVideoId != null && from.LastVideoId != null && to.LastVideoId != from.LastVideoId)
            {
                return CombineResult.False("have diff last video.");
            }

            if (to.NextVideoId != null && from.NextVideoId != null && to.NextVideoId != from.NextVideoId)
            {
                return CombineResult.False("have diff next video.");
            }

            if (to.DoubanId != null && from.DoubanId != null && to.DoubanId != from.DoubanId)
            {
                return CombineResult.False("have diff douban id.");
            }

            if (to.ImdbId != null && from.ImdbId != null && to.ImdbId != from.ImdbId)
            {
                return CombineResult.False("have diff douban id.");
            }

            if (to.EpisodesCount != from.EpisodesCount)
            {
                return CombineResult.False("have diff episodes count.");
            }

            return CombineResult.True;
        }

        internal override async Task<CombineResult> CombineAsync(JryVideoInfo to, JryVideoInfo from)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (from == null) throw new ArgumentNullException(nameof(from));

            var result = await this.CanCombineAsync(to, from);

            if (result.CanCombine)
            {
                to.CombineFrom(from);
                await this.RemoveAsync(from.Id);
                await this.UpdateAsync(to);
            }

            return result;
        }
    }
}