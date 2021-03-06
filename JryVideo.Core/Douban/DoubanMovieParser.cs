﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Jasily.SDK.Douban.Entities;

namespace JryVideo.Core.Douban
{
    public sealed class DoubanMovieParser
    {
        private static readonly Regex Name1 = new Regex(@"^(.*)(?:Season(?: ?)(\d+))$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex Name2 = new Regex(@"^(.*)(?:第(.){1,2}季)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex Name3 = new Regex(@"^(\D*)(\d+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly List<char> Numbers =
            new List<char>(new[] { '零', '一', '二', '三', '四', '五', '六', '七', '八', '九', '十' });

        private readonly List<string> seriesNames = new List<string>();
        private readonly List<string> entityNames = new List<string>();

        private DoubanMovieParser(Movie json)
        {
            this.Load(json);
        }

        public IEnumerable<string> SeriesNames => this.seriesNames;

        public IEnumerable<string> EntityNames => this.entityNames;

        public string Index { get; private set; }

        public string EpisodesCount { get; private set; }

        public bool IsMovie { get; set; }

        private void Load(Movie json)
        {
            this.LoadName(json);

            this.IsMovie = json.SubType == "movie";

            // EpisodesCount
            if (json.EpisodesCount.IsNullOrWhiteSpace())
            {
                if (this.IsMovie) this.EpisodesCount = "1";
            }
            else
            {
                this.EpisodesCount = json.EpisodesCount;
            }

            this.seriesNames.Reset(this.seriesNames.Select(z => z.Trim()).ToArray());
            this.entityNames.Reset(this.entityNames.Select(z => z.Trim()).ToArray());
        }

        private void LoadName(Movie json)
        {
            foreach (var name in json.AllNames())
            {
                if (this.ParseSeriesName(name)) continue;
                
                var spliter = name.Split(new[] { ":", "：" }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (!this.ParseSeriesName(spliter[0]))
                {
                    this.seriesNames.Add(spliter[0]);
                }

                if (spliter.Length == 2)
                {
                    this.entityNames.Add(name);
                }
            }
        }

        private bool ParseSeriesName(string name)
        {
            var match = Name1.Match(name);
            if (match.Success)
            {
                this.seriesNames.Add(match.Groups[1].Value);
                this.Index = match.Groups[2].Value;
                return true;
            }

            match = Name2.Match(name);
            if (match.Success)
            {
                this.seriesNames.Add(match.Groups[1].Value);
                var index = match.Groups[2].Value;
                if (index.Length > 0)
                {
                    var n1 = Numbers.FindIndex(z => z == index[0]);
                    if (n1 >= 0)
                    {
                        if (index.Length < 2) // 1
                        {
                            this.Index = n1.ToString();
                        }
                        else // > 1
                        {
                            var n2 = Numbers.FindIndex(z => z == index[1]);
                            if (n2 == 10)
                            {
                                switch (index.Length)
                                {
                                    case 2:
                                        this.Index = (n1 + 10).ToString();
                                        break;
                                    case 3:
                                        var n3 = Numbers.FindIndex(z => z == index[2]);
                                        if (n3 > -1 && n3 < 10)
                                        {
                                            this.Index = (n3 * 10 + n1).ToString();
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                return true;
            }

            match = Name3.Match(name);
            if (match.Success)
            {
                this.seriesNames.Add(match.Groups[1].Value);
                this.Index = match.Groups[2].Value;
                return true;
            }

            return false;
        }

        public static DoubanMovieParser Parse(Movie json)
            => new DoubanMovieParser(json);
    }
}