using System.Collections.Generic;

namespace JryVideo.Configs
{
    public sealed class UserConfig
    {
        public DefaultValue DefaultValue { get; set; }

        public MapperConfig Mapper { get; set; }

        public List<string> SubTitleExtensions { get; set; }

        public int AutoDayOfWeekOffset { get; set; }

        public List<SearchEngineUrl> SearchEngines { get; set; }

        public class SearchEngineUrl
        {
            public string Name { get; set; }

            public string Url { get; set; }
        }
    }
}