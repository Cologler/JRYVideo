namespace JryVideo.Common
{
    public sealed class WatchedEpisodeChecker
    {
        public WatchedEpisodeChecker(int episode)
        {
            this.EpisodeName = $"ep {episode}";
        }

        public string EpisodeName { get; }

        public bool IsWatched { get; set; }
    }
}