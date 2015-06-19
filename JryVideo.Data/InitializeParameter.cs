using System;

namespace JryVideo.Data
{
    public struct InitializeParameter
    {
        public InitializeParameterType ParameterType;

        public string ParameterName;

        public object ParameterValue;

        public Func<object, bool> IsVaild;
    }
}