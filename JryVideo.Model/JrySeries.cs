using System;
using System.Collections.Generic;

namespace JryVideo.Model
{
    public sealed class JrySeries : JryObject
    {
        public JrySeries()
        {
            this.Names = new List<string>();
            this.Videos = new List<JryVideo>();
        }

        public List<string> Names { get; set; }

        public List<JryVideo> Videos { get; set; }

        public override IEnumerable<JryInvalidError> CheckError()
        {
            foreach (var error in base.CheckError())
            {
                yield return error;
            }

            if (this.Names == null)
            {
                yield return JryInvalidError.ObjectInitializeFailed;
            }
            else if (this.Names.Count == 0)
            {
                yield return JryInvalidError.SeriesNamesCanNotBeEmpty;
            }

            if (this.Videos == null)
            {
                yield return JryInvalidError.ObjectInitializeFailed;
            }
        }
    }
}