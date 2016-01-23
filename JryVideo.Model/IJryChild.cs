using System.Collections.Generic;

namespace JryVideo.Model
{
    public interface IJryChild<T>
    {
        List<T> Childs { get; }
    }
}