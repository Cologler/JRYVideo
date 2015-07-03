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

            var info = new Model.JryVideoInfo();
            info.Type = "Movie";
            info.DoubanId = "25851657";
            info.Index = 1;
            info.Year = 2005;
            info.EpisodesCount = 1;

            var series = new JrySeries();
            series.Names.AddRange(new [] { "series123", "series456" });
            series.Videos.Add(info);
            
            SeriesManager.BuildSeriesMetaData(series);

            if (!await manager.InsertAsync(series) && Debugger.IsAttached)
                Debugger.Break();

            var entity1 = new JryEntity();
            entity1.BuildMetaData();
            entity1.Resolution = "720P";
            entity1.Extension = "mp1";
            entity1.Fansubs.Add("A");

            var entity2 = new JryEntity();
            entity2.BuildMetaData();
            entity2.Resolution = "720P";
            entity2.Extension = "mp2";
            entity2.Fansubs.Add("A");
            entity2.Fansubs.Add("B");

            var entity3 = new JryEntity();
            entity3.BuildMetaData();
            entity3.Resolution = "1080P";
            entity3.Extension = "mp3";
            entity3.Fansubs.Add("A");
            entity3.Fansubs.Add("C");

            var video = await this.CurrentDataCenter.VideoManager.FindAsync(info.Id);
            video.Entities.AddRange(new []
            {
                entity1, entity2, entity3
            });

            await this.CurrentDataCenter.VideoManager.UpdateAsync(video);
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