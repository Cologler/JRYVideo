using System;

namespace JryVideo.Core.Managers
{
    public class CoverManager
    {
        public CoverManager()
        {
            
        }

        public byte[] this[Guid coverId]
        {
            get
            {
                if (coverId == Guid.Empty) return null;
                
                throw new NotImplementedException();
            }
        }
    }
}