using System;

namespace JryVideo.Data.Attributes
{
    public sealed class ParameterAttribute : Attribute
    {
        public bool IsOptional { get; set; }
    }
}