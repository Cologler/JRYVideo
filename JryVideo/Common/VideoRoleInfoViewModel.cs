using Jasily.ComponentModel;
using JryVideo.Core;
using JryVideo.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JryVideo.Common
{
    public class VideoRoleInfoViewModel : JasilyViewModel
    {
        private VideoRoleCollection info;

        public ObservableCollection<VideoRoleViewModel> MajorRoles { get; }
            = new ObservableCollection<VideoRoleViewModel>();

        public ObservableCollection<VideoRoleViewModel> MinorRoles { get; }
            = new ObservableCollection<VideoRoleViewModel>();

        public async void BeginLoad(string videoId)
        {
            this.MajorRoles.Clear();
            this.MinorRoles.Clear();

            this.info = await JryVideoCore.Current.CurrentDataCenter.VideoRoleManager.FindAsync(videoId);
            if (this.info != null)
            {
                if (this.info.MajorRoles != null)
                {
                    this.MajorRoles.AddRange(this.info.MajorRoles.Select(z => new VideoRoleViewModel(z)));
                }

                if (this.info.MinorRoles != null)
                {
                    this.MajorRoles.AddRange(this.info.MinorRoles.Select(z => new VideoRoleViewModel(z)));
                }
            }
        }
    }
}