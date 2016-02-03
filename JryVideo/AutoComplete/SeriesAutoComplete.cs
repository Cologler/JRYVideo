using JryVideo.Core;
using JryVideo.Core.Managers;
using JryVideo.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.AutoComplete
{
    public class SeriesAutoComplete
    {
        public async Task<bool> AutoCompleteAsync(SeriesManager manager, JrySeries series)
        {
            var haschanged = false;

            if (series.TheTVDBId.IsNullOrWhiteSpace())
            {
                var imdbId = series.GetValidImdbId();
                var client = JryVideoCore.Current.TheTVDBHost.LastClientInstance;
                if (client != null && imdbId != null)
                {
                    var item = (await client.GetSeriesByImdbIdAsync(imdbId)).FirstOrDefault();
                    if (item != null)
                    {
                        series.TheTVDBId = item.SeriesId;
                        haschanged = true;
                    }
                }
            }

            if (haschanged)
            {
                return await manager.UpdateAsync(series);
            }
            return false;
        }
    }
}