using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JryVideo.Model;
using JryVideo.Properties;

namespace JryVideo.Common
{
    public static class LocalizationHelper
    {
        public static string GetLocalizeString(this JryFlagType flag)
        {
            switch (flag)
            {
                case JryFlagType.VideoYear:
                    return Resources.JryFlagType_VideoYear;
                case JryFlagType.VideoType:
                    return Resources.JryFlagType_VideoType;
                case JryFlagType.EntityResolution:
                    return Resources.JryFlagType_EntityResolution;
                case JryFlagType.EntityFilmSource:
                    return Resources.JryFlagType_EntityFilmSource;
                case JryFlagType.EntityExtension:
                    return Resources.JryFlagType_EntityExtension;
                case JryFlagType.EntityFansub:
                    return Resources.JryFlagType_EntityFansub;
                case JryFlagType.EntitySubTitleLanguage:
                    return Resources.JryFlagType_EntitySubTitleLanguage;
                case JryFlagType.EntityTrackLanguage:
                    return Resources.JryFlagType_EntityTrackLanguage;
                case JryFlagType.EntityAudioSource:
                    return Resources.JryFlagType_EntityAudioSource;
                default:
                    throw new ArgumentOutOfRangeException("flag", flag, null);
            }
        }
    }
}
