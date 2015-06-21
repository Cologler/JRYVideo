using System;
using System.Collections.Generic;
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

            await manager.InsertAsync(new JrySeries()
            {
                Names = new List<string>()
                {
                    "series123", "series456"
                },

                Videos = new List<Model.JryVideo>()
                {
                    new Model.JryVideo()
                    {
                        Type = "Movie",

                        DoubanId = "25851657",

                        Index = 1,

                        Year = 2005,

                        Entities = new List<JryEntity>()
                        {
                            new JryEntity()
                            {
                                
                            }.InitializeInstance()
                        }
                    }.InitializeInstance()
                }
            }.InitializeInstance());
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