using System;

namespace JryVideo.Core.Managers
{
    public class CombineResult
    {
        public static CombineResult True { get; }
            = new CombineResultTrue();

        public static CombineResult NotSupported { get; }
            = False("not supported combine.");

        public static CombineResult NotFound { get; }
            = False("404: object not found.");

        protected CombineResult()
        {
            
        }

        public virtual bool CanCombine => false;

        public static CombineResult False(string message)
        {
            return new CombineResult()
            {
                Message = message
            };
        }

        public string Message { get; private set; }

        private class CombineResultTrue : CombineResult
        {
            public override bool CanCombine => true;
        }
    }
}