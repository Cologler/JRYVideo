using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Enums;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Core.Douban;
using JryVideo.Core.Managers;
using JryVideo.Model;

namespace JryVideo.Controls.EditSeries
{
    public class EditSeriesViewModel : JasilyViewModel
    {
        private string _names;
        private JrySeries _source;
        private string _doubanId;

        public event EventHandler<string[]> FindErrorMessages;
        public event EventHandler<JrySeries> Created;
        public event EventHandler<JrySeries> Updated;

        public EditSeriesViewModel()
        {
            this._names = "";
        }

        public ObjectChangedAction Action { get; set; }

        public JrySeries Source
        {
            get { return this._source; }
            set
            {
                this._source = value;
                this.Names = value == null || value.Names == null ? "" : value.Names.AsLines();
            }
        }

        public string Names
        {
            get { return this._names; }
            set { this.SetPropertyRef(ref this._names, value); }
        }

        public string DoubanId
        {
            get { return this._doubanId; }
            set { this.SetPropertyRef(ref this._doubanId, value); }
        }

        public DoubanMovie DoubanMovie { get; private set; }

        public async Task LoadDoubanAsync()
        {
            if (String.IsNullOrWhiteSpace(this.DoubanId)) return;

            this.DoubanMovie = await DoubanHelper.TryGetMovieInfoAsync(this.DoubanId);

            if (this.DoubanMovie != null)
            {
                this.Names = String.IsNullOrWhiteSpace(this.Names)
                    ? DoubanHelper.ParseName(this.DoubanMovie).AsLines()
                    : String.Join("\r\n", this.Names, DoubanHelper.ParseName(this.DoubanMovie).AsLines());
            }
        }

        public async Task<bool> CommitAsync()
        {
            JrySeries series;

            switch (this.Action)
            {
                case ObjectChangedAction.Create:
                    series = new JrySeries();
                    break;

                case ObjectChangedAction.Modify:
                    series = this.Source.ThrowIfNull("Source");
                    break;

                case ObjectChangedAction.Replace:
                case ObjectChangedAction.Delete:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            series.ThrowIfNull("series").Names.AddRange(
                this.Names.AsLines()
                    .Select(z => z.Trim())
                    .Where(z => !String.IsNullOrWhiteSpace(z)));

            series.Names = series.Names.Distinct().ToList();

            SeriesManager.BuildSeriesMetaData(series);

            var error = series.FireObjectError().ToArray();

            if (error.Length > 0)
            {
                this.FindErrorMessages.Fire(this, error);

                return false;
            }
            else
            {
                var seriesManager = JryVideoCore.Current.CurrentDataCenter.SeriesManager;

                switch (this.Action)
                {
                    case ObjectChangedAction.Create:
                        if (await seriesManager.InsertAsync(series))
                        {
                            this.Created.BeginFire(this, series);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                        break;

                    case ObjectChangedAction.Modify:
                        if (await seriesManager.UpdateAsync(series))
                        {
                            this.Updated.BeginFire(this, series);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                        break;

                    case ObjectChangedAction.Replace:
                    case ObjectChangedAction.Delete:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}