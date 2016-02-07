namespace JryVideo.Model
{
    public static class InterfaceExtension
    {
        public static string GetValidImdbId(this IImdbItem item)
        {
            return item?.ImdbId != null &&
                (item.ImdbId.StartsWith("tt") || item.ImdbId.StartsWith("nm"))
                ? item.ImdbId
                : null;
        }
    }
}