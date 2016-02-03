using System.Threading.Tasks;
using JryVideo.Core.Managers;
using JryVideo.Model;

namespace JryVideo.AutoComplete
{
    public class VideoInfoAutoComplete
    {
        public async Task<bool> AutoCompleteRoleAsync(VideoRoleManager manager, JryVideoInfo video)
        {
            var imdbId = video.GetValidImdbId();
            if (imdbId != null)
            {
                return await manager.AutoCreateVideoRoleAsync(video.Id, new RemoteId(RemoteIdType.Imdb, imdbId));
            }
            return false;
        }
    }
}