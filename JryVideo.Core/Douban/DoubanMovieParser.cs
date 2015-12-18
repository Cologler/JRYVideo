using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JryVideo.Core.Douban
{
    public sealed class DoubanMovieParser
    {
        private static readonly Regex Name1 = new Regex(@"^(.*)(?:Season(?: ?)(\d+))$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex Name2 = new Regex(@"^(.*)(?:第(?:.){1,2}季)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly List<string> seriesNames = new List<string>();
        private readonly List<string> entityNames = new List<string>();

        private DoubanMovieParser(DoubanMovie json)
        {
            this.Load(json);
        }

        public IEnumerable<string> SeriesNames => this.seriesNames;

        public IEnumerable<string> EntityNames => this.entityNames;

        public string Index { get; private set; }

        private void Load(DoubanMovie json)
        {
            this.LoadName(json);
        }

        private void LoadName(DoubanMovie json)
        {
            foreach (var name in json.ParseName())
            {
                var match = Name1.Match(name);
                if (match.Success)
                {
                    this.seriesNames.Add(match.Groups[1].Value);
                    this.Index = match.Groups[2].Value;
                    continue;
                }

                match = Name2.Match(name);
                if (match.Success)
                {
                    this.seriesNames.Add(match.Groups[1].Value);
                    continue;
                }

                var spliter = name.Split(new string[] { ":", "：" }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (spliter.Length > 0)
                {
                    this.seriesNames.Add(spliter[0]);
                    if (spliter.Length > 1) this.entityNames.Add(spliter[1]);
                }
            }
        }

        public static DoubanMovieParser Parse(DoubanMovie json)
            => new DoubanMovieParser(json);
    }
}