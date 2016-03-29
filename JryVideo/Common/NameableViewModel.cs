using Jasily.Chinese.PinYin;
using Jasily.ComponentModel;
using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Common
{
    public sealed class NameableViewModel<T> : JasilyViewModel<T>
        where T : INameable
    {
        private static readonly PinYinManager PinYinManager = PinYinManager.CreateInstance();
        private static readonly RefreshPropertiesMapper Mapper = new RefreshPropertiesMapper(typeof(NameableViewModel<T>));

        public NameableViewModel(T source)
            : base(source)
        {
            this.PropertiesMapper = Mapper;
            this.QueryStrings = Empty<string>.Array;
        }

        [NotifyPropertyChanged]
        public string FirstLine => this.Source.GetMajorName() ?? string.Empty;

        [NotifyPropertyChanged]
        public string SecondLine => this.Source.Names?.Count > 1 ? this.Source.Names[1] : null;

        /// <summary>
        /// like A \r\n B \r\n C
        /// </summary>
        [NotifyPropertyChanged]
        public string FullName => this.Source.Names?.AsLines() ?? string.Empty;

        [NotifyPropertyChanged]
        public string FullNameLine => this.Source.Names == null ? string.Empty : string.Join(" / ", this.Source.Names);

        public string[] QueryStrings { get; private set; }

        /// <summary>
        /// the method will call PropertyChanged for each property which has [NotifyPropertyChanged]
        /// </summary>
        public override void RefreshProperties()
        {
            base.RefreshProperties();
            if (this.IsBuildQueryStrings) this.BeginRebuildQueryStrings();
        }

        public bool IsBuildQueryStrings { get; set; }

        public void RebuildQueryStrings()
        {
            var names = this.Source.Names?.ToArray();
            if (names == null) return;
            var f = new List<string>(names.Length);
            foreach (var name in names)
            {
                // pinyin
                var array = name.ToCharArray();
                var diff = false;
                for (var j = 0; j < array.Length; j++)
                {
                    var py = PinYinManager[array[j]];
                    if (py.HasValue && py.Value.PinYin.Length > 0)
                    {
                        array[j] = py.Value.PinYin[0];
                        diff = true;
                    }
                }
                if (diff) f.Add(array.GetString());

                // english
                if (name.All(z => z.IsEnglishChar()))
                {
                    // super girl => sg
                    var words = name.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 1)
                    {
                        f.Add(words.Select(z => z[0]).GetString());
                    }

                    // SuperGirl => sg
                    var words2 = name.Where(char.IsUpper).GetString();
                    if (words2.Length != name.Length && words2.Length > 1)
                    {
                        f.Add(words2);
                    }
                }
            }
            this.QueryStrings = f.Count > 0 ? f.Distinct().ToArray() : Empty<string>.Array;
        }

        public void BeginRebuildQueryStrings() => Task.Run(() => this.RebuildQueryStrings());
    }
}