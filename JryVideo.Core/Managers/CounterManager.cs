using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class CounterManager
    {
        public ICounterDataSourceProvider Source { get; private set; }

        public CounterManager(ICounterDataSourceProvider source)
        {
            this.Source = source;
        }

        public async Task<IEnumerable<JryCounter>> LoadAsync(JryCounterType type)
        {
            return await this.Source.QueryAsync(type);
        }
    }
}