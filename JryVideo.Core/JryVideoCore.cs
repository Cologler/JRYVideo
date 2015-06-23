using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JryVideo.Core.Douban;
using JryVideo.Core.Managers;
using JryVideo.Data;
using JryVideo.Model;

namespace JryVideo.Core
{
    public class JryVideoCore
    {
        public static JryVideoCore Current { get; private set; }

        static JryVideoCore()
        {
            Current = new JryVideoCore();
        }

        public async Task InitializeAsync()
        {
            var dataSourceManager = DataSourceManager.Default;
            dataSourceManager.Scan();

            var normal = dataSourceManager.GetDefault();
            foreach (var initializeParameter in normal.InitializeParametersInfo.GetRequiredParameters())
            {
                var para = initializeParameter;
                para.ParameterValue = "";
                normal.InitializeParametersInfo.SetInitializeParameter(initializeParameter);
            }
            await normal.Initialize(JryVideoDataSourceProviderManagerMode.Public);
            this.NormalDataCenter = new DataCenter(normal);

            var secure = dataSourceManager.GetDefault();
            foreach (var initializeParameter in secure.InitializeParametersInfo.GetRequiredParameters())
            {
                var para = initializeParameter;
                para.ParameterValue = "";
                secure.InitializeParametersInfo.SetInitializeParameter(initializeParameter);
            }
            await secure.Initialize(JryVideoDataSourceProviderManagerMode.Private);
            this.SecureDataCenter = new DataCenter(secure);

            this.Switch(JryVideoDataSourceProviderManagerMode.Public);

            await this.TestForAllItem();
        }

        private async Task TestForAllItem()
        {
            var manager = this.CurrentDataCenter.SeriesManager;

            var entity = new JryEntity();

            var video = new Model.JryVideo();
            video.Entities.Add(entity);
            video.Type = "Movie";
            video.DoubanId = "25851657";
            video.Index = 1;
            video.Year = 2005;

            var series = new JrySeries();
            series.Names.AddRange(new [] { "series123", "series456" });
            series.Videos.Add(video);

            SeriesManager.BuildSeriesMetaData(series);

            if (!await manager.InsertAsync(series) && Debugger.IsAttached)
                Debugger.Break();
        }

        public DataCenter NormalDataCenter { get; private set; }

        public DataCenter SecureDataCenter { get; private set; }

        public DataCenter CurrentDataCenter { get; private set; }

        public void Switch(JryVideoDataSourceProviderManagerMode mode)
        {
            switch (mode)
            {
                case JryVideoDataSourceProviderManagerMode.Public:
                    this.CurrentDataCenter = this.NormalDataCenter;
                    break;

                case JryVideoDataSourceProviderManagerMode.Private:
                    this.CurrentDataCenter = this.SecureDataCenter;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("mode", mode, null);
            }
        }
    }
}