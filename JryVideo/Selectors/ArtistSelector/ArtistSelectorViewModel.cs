using System;
using System.Linq;
using JryVideo.Common;
using JryVideo.Selectors.Common;

namespace JryVideo.Selectors.ArtistSelector
{
    public sealed class ArtistSelectorViewModel : BaseSelectorViewModel<ArtistViewModel>
    {
        protected override bool OnFilter(ArtistViewModel obj)
        {
            if (String.IsNullOrWhiteSpace(this.FilterText)) return true;

            if (obj == null) return true;

            var keyword = this.FilterText.Trim().ToLower();

            return obj.Source.Names.Any(z => z.ToLower().Contains(keyword));
        }
    }
}