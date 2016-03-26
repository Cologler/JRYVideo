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
            this.Pinyins = Empty<string>.Array;
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

        public string[] Pinyins { get; private set; }

        /// <summary>
        /// the method will call PropertyChanged for each property which has [NotifyPropertyChanged]
        /// </summary>
        public override void RefreshProperties()
        {
            base.RefreshProperties();
            if (this.IsBuildPinyin) this.BeginRebuildPinyins();
        }

        public bool IsBuildPinyin { get; set; }

        public void RebuildPinyins()
        {
            var names = this.Source.Names?.ToArray();
            if (names == null) return;
            var f = new List<string>(names.Length);
            foreach (var array in names.Select(z => z.ToCharArray()))
            {
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
            }
            this.Pinyins = f.Count > 0 ? f.ToArray() : Empty<string>.Array;
        }

        public void BeginRebuildPinyins() => Task.Run(() => this.RebuildPinyins());
    }
}