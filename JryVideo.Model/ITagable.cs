using System.Collections.Generic;

namespace JryVideo.Model
{
    public interface ITagable
    {
        List<string> Tags { get; set; }
    }
}