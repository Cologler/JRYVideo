using Jasily.ComponentModel;
using Jasily.EverythingSDK;
using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Common
{
    public sealed class EntityViewModel : JasilyViewModel<JryEntity>
    {
        public EntityViewModel(JryEntity source)
            : base(source)
        {
            this.RefreshProperties();
        }

        [NotifyPropertyChanged]
        public string DisplayProperties => this.Source.Extension.ToUpper() +
            (this.Source.FilmSource == null ? string.Empty : ("-" + this.Source.FilmSource));

        [NotifyPropertyChanged]
        public string DisplayFansubs => ListToLine(this.Source.Fansubs);

        [NotifyPropertyChanged]
        public string DisplaySubTitleLanguages => ListToLine(this.Source.SubTitleLanguages);

        [NotifyPropertyChanged]
        public bool HasSubTitleLanguages => this.Source.SubTitleLanguages.Count > 0;

        [NotifyPropertyChanged]
        public bool DontHasSubTitleLanguages => this.Source.SubTitleLanguages.Count == 0;

        [NotifyPropertyChanged]
        public string DisplayTrackLanguages => ListToLine(this.Source.TrackLanguages);

        [NotifyPropertyChanged]
        public string DisplayTags => ListToLine(this.Source.Tags);

        [NotifyPropertyChanged]
        public string DisplayFormat => this.Source.Format == null
            ? null
            : string.Format("({0}), {1}", this.Source.Format.Type, this.Source.Format.Value);

        private static string ListToLine(List<string> lines) => lines.Count > 0 ? lines.AsLines(" / ") : "[EMPTY]";

        public async Task<IEnumerable<string>> SearchByEveryThingAsync()
        {
            var format = this.Source.Format;
            if (format == null) return Enumerable.Empty<string>();

            return await Task.Run(() =>
            {
                var search = new EverythingSearch();
                search.Parameters.IsMatchPath = false;
                search.Parameters.IsMatchWholeWord = false;
                search.Parameters.IsMatchCase = false;
                switch (format.Type)
                {
                    case JryFormatType.Regex:
                        search.Parameters.IsRegex = true;
                        break;

                    case JryFormatType.Wildcard:
                        search.Parameters.IsRegex = false;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return search.SearchAll(format.Value, 100).SelectMany(z => z).ToArray();
            });
        }
    }
}