using JryVideo.Model;
using JryVideo.Properties;
using System;

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
                case JryFlagType.ResourceResolution:
                    return Resources.JryFlagType_EntityResolution;
                case JryFlagType.ResourceQuality:
                    return Resources.JryFlagType_EntityFilmSource;
                case JryFlagType.ResourceExtension:
                    return Resources.JryFlagType_EntityExtension;
                case JryFlagType.ResourceFansub:
                    return Resources.JryFlagType_EntityFansub;
                case JryFlagType.ResourceSubTitleLanguage:
                    return Resources.JryFlagType_EntitySubTitleLanguage;
                case JryFlagType.ResourceTrackLanguage:
                    return Resources.JryFlagType_EntityTrackLanguage;
                case JryFlagType.ResourceAudioSource:
                    return Resources.JryFlagType_EntityAudioSource;
                case JryFlagType.ResourceTag:
                    return Resources.JryFlagType_EntityTag;
                case JryFlagType.SeriesTag:
                    return Resources.JryFlagType_SeriesTag;
                case JryFlagType.VideoTag:
                    return Resources.JryFlagType_VideoTag;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flag), flag, null);
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
                    throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null);
            }
        }

        public static string GetLocalizeString(this DayOfWeek? dayOfWeek)
        {
            return dayOfWeek.HasValue ? dayOfWeek.Value.GetLocalizeString() : Resources.DayOfWeek_Unknown;
        }

        public static string GetDataSourceField(string field)
        {
            return field;
        }
    }
}
