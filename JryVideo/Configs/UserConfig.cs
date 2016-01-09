using System.Collections.Generic;

namespace JryVideo.Configs
{
    public sealed class UserConfig
    {
        public DefaultValue DefaultValue { get; set; }

        public MapperConfig Mapper { get; set; }

        public List<string> SubTitleExtensions { get; set; }

        public int AutoDayOfWeekOffset { get; set; }
    }
}