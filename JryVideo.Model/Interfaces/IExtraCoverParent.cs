using System.Collections.Generic;

namespace JryVideo.Model.Interfaces
{
    public interface IExtraCoverParent
    {
        IEnumerable<string> GetExtraCoverIds();
    }
}