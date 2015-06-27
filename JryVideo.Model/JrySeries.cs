using System;
using System.Collections.Generic;

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

        public override IEnumerable<JryInvalidError> CheckError()
        {
            foreach (var error in base.CheckError())
            {
                yield return error;
            }

            if (this.Names == null || this.Videos == null)
            {
                throw new ArgumentException();
            }
            
            
            if (this.Names.Count == 0)
            {
                yield return JryInvalidError.SeriesNamesCanNotBeEmpty;
            }
        }
    }
}