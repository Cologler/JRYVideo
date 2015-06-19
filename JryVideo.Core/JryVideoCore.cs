using System;
using System.Collections.Generic;
using JryVideo.Core.Managers;
using JryVideo.Model;
using JryVideo = JryVideo.Model.JryVideo;

namespace JryVideo.Core
{
    public class JryVideoCore
    {
        public static void Initialize()
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
            source.Initialize();

            var data = source.GetSeriesDataSourceProvider();
            data.Put(new JrySeries()
            {
                Videos = new List<Model.JryVideo>()
                {
                    new Model.JryVideo()
                    {
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
    }
}