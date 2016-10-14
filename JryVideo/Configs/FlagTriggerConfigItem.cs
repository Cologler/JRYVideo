using System.Collections.Generic;

namespace JryVideo.Configs
{
    public class FlagTriggerConfigItem
    {
        public int FlagType { get; set; }

        public List<string> MatchExtensions { get; set; }

        public List<string> MatchStrings { get; set; }

        public List<string> Values { get; set; }
    }
}