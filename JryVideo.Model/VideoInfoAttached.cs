namespace JryVideo.Model
{
    /// <summary>
    /// 附加到 VideoInfo 上的对象，其 Id 与 VideoInfo 一致
    /// </summary>
    public abstract class VideoInfoAttached : RootObject
    {
        protected override void BuildId() { }

        public static T Build<T>(string id) where T : VideoInfoAttached, new()
        {
            var obj = new T { Id = id };
            obj.BuildMetaData(true);
            return obj;
        }
    }
}