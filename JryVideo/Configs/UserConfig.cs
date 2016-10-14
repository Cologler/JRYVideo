using System;
using System.Collections.Generic;
using System.Linq;
using JryVideo.Model;

namespace JryVideo.Configs
{
    public sealed class UserConfig
    {
        public DefaultValue DefaultValue { get; set; }

        public MapperConfig Mapper { get; set; }

        public List<string> SubTitleExtensions { get; set; }

        public int AutoDayOfWeekOffset { get; set; }

        public List<SearchEngineUrl> SearchEngines { get; set; }

        public List<FlagTriggerConfigItem> FlagTriggers { get; set; }

        public IEnumerable<FlagTriggerConfigItem> GetFlagTriggers(JryFlagType type)
            => this.FlagTriggers?.Where(z => z.FlagType == (int) type).EmptyIfNull();

        public class SearchEngineUrl
        {
            public string Name { get; set; }

            public string Url { get; set; }
        }
    }
}