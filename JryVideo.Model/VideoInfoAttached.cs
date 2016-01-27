namespace JryVideo.Model
{
    public abstract class VideoInfoAttached : JryObject
    {
        protected override string BuildId() => this.Id;

        public static T Build<T>(JryVideoInfo info) where T : VideoInfoAttached, new()
            => Build<T>(info.Id);

        public static T Build<T>(string id) where T : VideoInfoAttached, new()
        {
            var obj = new T() { Id = id };
            obj.BuildMetaData(true);
            return obj;
        }
    }
}