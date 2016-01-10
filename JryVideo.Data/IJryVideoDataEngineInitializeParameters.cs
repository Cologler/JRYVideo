namespace JryVideo.Data
{
    public interface IJryVideoDataEngineInitializeParameters
    {
        bool IsVaild(string fieldName, object value);
    }
}