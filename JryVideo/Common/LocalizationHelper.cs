using System;
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
                case JryFlagType.EntityTag:
                    return Resources.JryFlagType_EntityTag;
                default:
                    throw new ArgumentOutOfRangeException("flag", flag, null);
            }
        }

        public static string GetLocalizeString(this DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return Resources.DayOfWeek_Sunday;
                case DayOfWeek.Monday:
                    return Resources.DayOfWeek_Monday;
                case DayOfWeek.Tuesday:
                    return Resources.DayOfWeek_Tuesday;
                case DayOfWeek.Wednesday:
                    return Resources.DayOfWeek_Wednesday;
                case DayOfWeek.Thursday:
                    return Resources.DayOfWeek_Thursday;
                case DayOfWeek.Friday:
                    return Resources.DayOfWeek_Friday;
                case DayOfWeek.Saturday:
                    return Resources.DayOfWeek_Saturday;
                default:
                    throw new ArgumentOutOfRangeException("dayOfWeek", dayOfWeek, null);
            }
        }

        public static string GetLocalizeString(this DayOfWeek? dayOfWeek)
        {
            return dayOfWeek.HasValue ? dayOfWeek.Value.GetLocalizeString() : Resources.DayOfWeek_Unknown;
        }
    }
}
