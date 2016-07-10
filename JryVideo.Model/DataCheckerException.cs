using System;

namespace JryVideo.Model
{
    public class DataCheckerException : Exception
    {
        public DataCheckerException(string message)
            : base(message)
        {

        }
    }
}