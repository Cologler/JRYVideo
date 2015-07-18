using System;
using System.Threading.Tasks;
using System.Windows.Data;
using JryVideo.Core;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class ArtistViewModel : HasCoverViewModel<JryArtist>
    {
        private string _doubanId;
        private string _name;
        private string _description;

        public ArtistViewModel(JryArtist source)
            : base(source)
        {
            this._doubanId = this.Source.DoubanId;
            this._name = String.Join(" / ", this.Source.Names);
            this._description = this.Source.Description;
        }

        /// <summary>
        /// may null.
        /// </summary>
        public string DoubanId
        {
            get { return this._doubanId; }
            set { this.SetPropertyRef(ref this._doubanId, value); }
        }

        public string Name
        {
            get { return this._name; }
            set { this.SetPropertyRef(ref this._name, value); }
        }

        /// <summary>
        /// may null.
        /// </summary>
        public string Description
        {
            get { return this._description; }
            set { this.SetPropertyRef(ref this._description, value); }
        }

        protected override async Task<bool> TryAutoAddCoverAsync()
        {
            if (this.Source.DoubanId == null) return false;

            var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;

            var guid = await coverManager.GetCoverFromDoubanIdAsync(JryCoverType.Artist, this.Source.DoubanId);

            if (guid == null) return false;

            this.Source.CoverId = guid;

            var artistManager = JryVideoCore.Current.CurrentDataCenter.ArtistManager;
            await artistManager.UpdateAsync(this.Source);
            return true;
        }
    }
}