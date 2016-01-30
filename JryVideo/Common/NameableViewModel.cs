using Jasily.ComponentModel;
using JryVideo.Model;
using System;
using System.Linq;

namespace JryVideo.Common
{
    public class NameableViewModel<T> : JasilyViewModel<T>
        where T : INameable
    {
        public NameableViewModel(T source)
            : base(source)
        {
        }

        [NotifyPropertyChanged]
        public string FirstLine => this.Source.Names?.FirstOrDefault() ?? string.Empty;

        [NotifyPropertyChanged]
        public string SecondLine => this.Source.Names?.Count > 1 ? this.Source.Names[1] : null;

        /// <summary>
        /// like A \r\n B \r\n C
        /// </summary>
        [NotifyPropertyChanged]
        public string FullName => this.Source.Names?.AsLines() ?? string.Empty;
    }
}