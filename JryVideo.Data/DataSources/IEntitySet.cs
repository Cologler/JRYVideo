using Jasily.Data;

namespace JryVideo.Data.DataSources
{
    public interface IEntitySet<T> : IJasilyEntitySetProvider<T, string>
        where T : class, IJasilyEntity<string>
    {

    }
}