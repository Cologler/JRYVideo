namespace JryVideo.Data.DataSources
{
    public interface IDataSourceWriteProvider<in T>
    {
        void Put(T value);
    }
}