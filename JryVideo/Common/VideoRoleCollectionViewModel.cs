using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.AutoComplete;
using JryVideo.Core;
using JryVideo.Model;
using JryVideo.Viewer.VideoViewer;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JryVideo.Common
{
    public class VideoRoleCollectionViewModel : JasilyViewModel, IComparer
    {
        private readonly JrySeries series;
        private readonly JryVideoInfo video;

        public VideoRoleCollectionViewModel(JrySeries series, JryVideoInfo video)
        {
            this.series = series;
            this.video = video;

            this.Roles.View.CustomSort = this;
            this.Roles.View.GroupDescriptions?.Add(new PropertyGroupDescription(nameof(VideoRoleViewModel.GroupTitle)));
        }

        public VideoViewerViewModel VideoViewerViewModel { get; set; }

        public JasilyCollectionView<VideoRoleViewModel> Roles { get; }
            = new JasilyCollectionView<VideoRoleViewModel>();

        public List<VideoRoleCollection> VideoRoleCollectionSources { get; }
            = new List<VideoRoleCollection>();

        public async Task AutoCompleteAsync()
        {
            var acs = await new SeriesAutoComplete()
                .AutoCompleteRoleAsync(this.GetManagers().VideoRoleManager, this.series);
            var acv = await new VideoInfoAutoComplete()
                .AutoCompleteRoleAsync(this.GetManagers().VideoRoleManager, this.video);
            if (acs || acv)
            {
                await this.LoadAsync();
            }
        }

        public async Task LoadAsync()
        {
            this.Roles.Collection.Clear();
            this.VideoRoleCollectionSources.Clear();

            var major = new List<VideoRoleViewModel>();
            var minor = new List<VideoRoleViewModel>();
            foreach (var imdbItem in new IImdbItem[] { this.series, this.video })
            {
                var col = await this.GetManagers().VideoRoleManager.FindAsync(imdbItem.Id);
                if (col != null)
                {
                    this.VideoRoleCollectionSources.Add(col);
                    if (col.MajorRoles != null)
                    {
                        major.AddRange(col.MajorRoles.Select(z => new VideoRoleViewModel(z, this, imdbItem, true)));
                    }
                    if (col.MinorRoles != null)
                    {
                        minor.AddRange(col.MinorRoles.Select(z => new VideoRoleViewModel(z, this, imdbItem, false)));
                    }
                }
            }

            this.Roles.Collection.AddRange(major);
            this.Roles.Collection.AddRange(minor);
            this.Roles.Collection.ForEach(z => z.RefreshProperties());
        }

        public async Task CommitAsync(string id = null)
        {
            foreach (var col in this.VideoRoleCollectionSources.Where(z => id == null || z.Id == id))
            {
                await this.GetManagers().VideoRoleManager.UpdateAsync(col);
            }
        }

        public async Task DeleteAsync(VideoRoleViewModel role)
        {
            Debug.Assert(this.VideoRoleCollectionSources.Count == 2);
            var series = this.VideoRoleCollectionSources[0];
            var video = this.VideoRoleCollectionSources[1];
            var collection = role.IsSeriesRole ? series : video;
            if (collection.MajorRoles?.Remove(role.Source) == true ||
                collection.MinorRoles?.Remove(role.Source) == true)
            {
                this.Roles.Collection.Remove(role);
                await this.GetManagers().VideoRoleManager.UpdateAsync(collection);
                var id = role.Source.CoverId;
                if (!string.IsNullOrWhiteSpace(id))
                {
                    await this.GetManagers().CoverManager.RemoveAsync(id);
                }
            }
        }

        public async Task<IImdbItem> MoveToAnotherCollectionAsync(VideoRoleViewModel role)
        {
            Debug.Assert(this.VideoRoleCollectionSources.Count == 2);
            var series = this.VideoRoleCollectionSources[0];
            var video = this.VideoRoleCollectionSources[1];
            if (role.IsSeriesRole ? Exchange(series, video, role.Source) : Exchange(video, series, role.Source))
            {
                await JryVideoCore.Current.CurrentDataCenter.VideoRoleManager.UpdateAsync(series);
                await JryVideoCore.Current.CurrentDataCenter.VideoRoleManager.UpdateAsync(video);
                return role.IsSeriesRole ? this.video : this.series as IImdbItem;
            }
            return null;
        }

        private static bool Exchange(VideoRoleCollection source, VideoRoleCollection dest, JryVideoRole role)
        {
            if (source.MajorRoles?.Contains(role) == true)
            {
                source.MajorRoles.Remove(role);
                (dest.MajorRoles ?? (dest.MajorRoles = new List<JryVideoRole>())).Add(role);
                return true;
            }
            if (source.MinorRoles?.Contains(role) == true)
            {
                source.MinorRoles.Remove(role);
                (dest.MinorRoles ?? (dest.MinorRoles = new List<JryVideoRole>())).Add(role);
                return true;
            }
            return false;
        }

        public int Compare(object x, object y) => this.Compare(x as VideoRoleViewModel, y as VideoRoleViewModel);

        public int Compare(VideoRoleViewModel x, VideoRoleViewModel y) => x.OrderIndex.CompareTo(y.OrderIndex);
    }
}