using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JryVideo.Model
{
    public sealed class JrySeries : JryObject
    {
        public JrySeries()
        {
            this.Names = new List<string>();
            this.Videos = new List<JryVideoInfo>();
        }

        public List<string> Names { get; set; }

        public List<JryVideoInfo> Videos { get; set; }

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.Names == null || this.Videos == null)
            {
                throw new ArgumentException();
            }
            
            if (this.Names.Count == 0)
            {
                JasilyLogger.Current.WriteLine<JrySeries>(JasilyLogger.LoggerMode.Debug, "series name can not be empty.");
                return true;
            }

            return false;
        }
    }
}