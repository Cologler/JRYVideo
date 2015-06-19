using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Core.Douban;
using JryVideo.Core.Managers;
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
            var source = dataSourceManager.GetDefault();
            foreach (var initializeParameter in source.InitializeParametersInfo.GetRequiredParameters())
            {
                var para = initializeParameter;
                para.ParameterValue = "";
                source.InitializeParametersInfo.SetInitializeParameter(initializeParameter);
            }
            await source.Initialize();

            var data = source.GetSeriesDataSourceProvider();
            await data.InsertAsync(new JrySeries()
            {
                Names = new List<string>()
                {
                    "series123", "series456"
                },

                Videos = new List<Model.JryVideo>()
                {
                    new Model.JryVideo()
                    {
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

            this.CoverManager = new CoverManager(source.GetCoverDataSourceProvider());
            this.SeriesManager = new SeriesManager(source.GetSeriesDataSourceProvider());
        }

        public CoverManager CoverManager { get; private set; }

        public SeriesManager SeriesManager { get; private set; }
    }
}