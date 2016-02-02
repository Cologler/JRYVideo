namespace JryVideo.Model
{
    public struct RemoteId
    {
        public RemoteId(RemoteIdType type, string id)
        {
            this.Type = type;
            this.Id = id;
        }

        public RemoteIdType Type { get; }

        public string Id { get; }
    }
}