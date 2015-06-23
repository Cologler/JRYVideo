using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class CounterManager : JryObjectManager<JryCounter, ICounterDataSourceProvider>
    {
        public CounterManager(ICounterDataSourceProvider source)
            : base(source)
        {
        }

        public async Task<IEnumerable<JryCounter>> LoadAsync(JryCounterType type)
        {
            return await this.Source.QueryAsync(type);
        }
    }
}