using System;
using System.Collections.Generic;

namespace JryVideo.Model
{
    public sealed class JrySeries : JryObject, IInitializable<JrySeries>
    {
        public List<string> Names { get; set; }

        public List<JryVideo> Videos { get; set; }

        public JrySeries InitializeInstance(JrySeries obj)
        {
            obj.Names = new List<string>();
            obj.Videos = new List<JryVideo>();

            return base.InitializeInstance(obj);
        }

        public override IEnumerable<string> CheckError()
        {
            foreach (var error in base.CheckError())
            {
                yield return error;
            }

            if (this.Names == null)
            {
                yield return "error Names";
            }
            else if (this.Names.Count == 0)
            {
                yield return "error Names Count";
            }

            if (this.Videos == null)
            {
                yield return "error Videos";
            }
        }
    }
}