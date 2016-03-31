using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Jasily.ComponentModel;
using JryVideo.Model;

namespace JryVideo.Common
{
    public sealed class VideoGroupingViewModel : JasilyViewModel
    {
        public ObservableCollection<FlagViewModel> Tags { get; } = new ObservableCollection<FlagViewModel>();

        public ObservableCollection<FlagViewModel> Years { get; } = new ObservableCollection<FlagViewModel>();

        public ObservableCollection<FlagViewModel> Types { get; } = new ObservableCollection<FlagViewModel>();

        public List<int> Stars { get; } = new List<int>() { 1, 2, 3, 4, 5};

        public async Task ReloadFlagsAsync(params JryFlagType[] flags)
        {
            var manager = this.GetManagers().FlagManager;

            var tags = Enumerable.Empty<JryFlag>();

            foreach (var flag in flags.Distinct())
            {
                switch (flag)
                {
                    case JryFlagType.VideoYear:
                        var years = await manager.LoadAsync(JryFlagType.VideoYear);
                        this.Years.Reset(years.Select(z => new FlagViewModel(z)));
                        break;

                    case JryFlagType.VideoType:
                        var type = await manager.LoadAsync(JryFlagType.VideoType);
                        this.Types.Reset(type.Select(z => new FlagViewModel(z)));
                        break;

                    case JryFlagType.SeriesTag:
                        tags = tags.Concat(await manager.LoadAsync(JryFlagType.SeriesTag));
                        break;

                    case JryFlagType.VideoTag:
                        tags = tags.Concat(await manager.LoadAsync(JryFlagType.VideoTag));
                        break;

                    case JryFlagType.EntityResolution:
                    case JryFlagType.EntityQuality:
                    case JryFlagType.EntityExtension:
                    case JryFlagType.EntityFansub:
                    case JryFlagType.EntitySubTitleLanguage:
                    case JryFlagType.EntityTrackLanguage:
                    case JryFlagType.EntityAudioSource:
                    case JryFlagType.EntityTag:
                        throw new NotSupportedException();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            this.Tags.Reset(tags.Select(z => new FlagViewModel(z)));
        }
    }
}