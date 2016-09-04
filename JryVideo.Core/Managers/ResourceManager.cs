using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public sealed class ResourceManager : JryObjectManager<Resource, IResourceDataSource>
    {
        public ResourceManager(IResourceDataSource source)
            : base(source)
        {
        }

        public void Initialize(DataCenter dataCenter)
        {
            dataCenter.SeriesManager.VideoInfoRemoved += this.SeriesManager_VideoInfoRemoved;
            dataCenter.FlagManager.FlagChanged += this.FlagManager_FlagChanged;
        }

        [Obsolete("use RemoveAsync(JryVideoInfo, Resource) or RemoveAsync(JryVideoInfo)")]
        public override Task<bool> RemoveAsync(Resource obj)
        {
            throw new NotSupportedException();
        }

        public async Task<bool> RemoveAsync(JryVideoInfo video, Resource resource)
        {
            resource.VideoIds = resource.VideoIds.Where(z => z != video.Id).ToList();
            if (resource.VideoIds.Count == 0)
            {
                await base.RemoveAsync(resource);
            }
            else
            {
                await this.UpdateAsync(resource);
            }
            return true;
        }

        public async Task<bool> RemoveAsync(JryVideoInfo video)
        {
            var resources = await this.QueryByVideoIdAsync(video.Id);
            foreach (var resource in resources)
            {
                await this.RemoveAsync(video, resource);
            }
            return true;
        }

        private async void SeriesManager_VideoInfoRemoved(object sender, IEnumerable<JryVideoInfo> e)
        {
            await Task.WhenAll(e.Select(async z => await this.RemoveAsync(z)));
        }

        private async void FlagManager_FlagChanged(object sender, EventArgs<JryFlagType, string, string> e)
        {
            var type = e.Value1;
            switch (type)
            {
                case JryFlagType.ResourceFansub:
                case JryFlagType.ResourceSubTitleLanguage:
                case JryFlagType.ResourceTrackLanguage:
                case JryFlagType.ResourceTag:
                    break;

                default:
                    if ((int)type > 20) throw new NotSupportedException();
                    return;
            }

            var oldValue = e.Value2;
            var newValue = e.Value3;
            var resources = await this.Source.QueryAsync(new Resource.QueryParameter { FlagType = type, FlagValue = oldValue }, 0, int.MaxValue);
            Action<List<string>> changeValue = (z) =>
            {
                var a = z.ToArray();
                for (var i = 0; i < a.Length; i++)
                {
                    if (a[i] == oldValue)
                    {
                        z[i] = newValue;
                        return;
                    }
                }
            };
            Action<Resource> changing;
            switch (type)
            {
                case JryFlagType.ResourceFansub:
                    changing = z => changeValue(z.Fansubs);
                    break;

                case JryFlagType.ResourceSubTitleLanguage:
                    changing = z => changeValue(z.SubTitleLanguages);
                    break;

                case JryFlagType.ResourceTrackLanguage:
                    changing = z => changeValue(z.TrackLanguages);
                    break;

                case JryFlagType.ResourceTag:
                    changing = z => changeValue(z.Tags);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var resource in resources)
            {
                changing(resource);
                await this.UpdateAsync(resource);
            }
        }

        public Task<IEnumerable<Resource>> QueryByVideoIdAsync(string videoId)
            => this.Source.QueryAsync(new Resource.QueryParameter { VideoId = videoId }, 0, int.MaxValue);
    }
}