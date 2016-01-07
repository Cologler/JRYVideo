namespace JryVideo.Common
{
    public sealed class WatchedEpisodeChecker
    {
        public WatchedEpisodeChecker(int episode)
        {
            this.Episode = episode;
            this.EpisodeName = $"ep {episode}";
        }

        public int Episode { get; }

        public string EpisodeName { get; }

        public bool IsWatched { get; set; }
    }
}