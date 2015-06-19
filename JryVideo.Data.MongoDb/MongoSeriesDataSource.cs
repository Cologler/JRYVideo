using System;
using System.Collections.Generic;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Data.MongoDb
{
    public class MongoSeriesDataSource : IDataSourceProvider<JrySeries>
    {
        public IEnumerable<JrySeries> Get(int skip = 0, int take = Int32.MaxValue)
        {
            throw new System.NotImplementedException();
        }

        public void Put(JrySeries value)
        {
            throw new System.NotImplementedException();
        }
    }
}