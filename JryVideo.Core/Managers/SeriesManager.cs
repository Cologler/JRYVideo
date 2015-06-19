using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class SeriesManager
    {
        public IDataSourceProvider<JrySeries> Source { get; private set; }

        public SeriesManager(IDataSourceProvider<JrySeries> source)
        {
            this.Source = source;
        }

        public async Task<IEnumerable<JrySeries>> LoadAsync()
        {
            return await this.Source.QueryAsync(0, Int32.MaxValue);
        }

        public async Task<bool> UpdateAsync(JrySeries update)
        {
            return await this.Source.UpdateAsync(update);
        }
    }
}