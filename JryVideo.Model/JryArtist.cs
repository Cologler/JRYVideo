using System;
using System.Collections.Generic;
using JryVideo.Model.JryInterfaces;

namespace JryVideo.Model
{
    public class JryArtist : JryObject, ICounterable
    {
        public string DoubanId { get; set; }

        public string Name { get; set; }

        public JryObject InitializeInstance(JryObject obj)
        {
            return base.InitializeInstance(obj);
        }

        public override IEnumerable<string> CheckError()
        {
            foreach (var error in base.CheckError())
            {
                yield return error;
            }

            foreach (var error in ((ICounterable)this).CheckError())
            {
                yield return error;
            }

            if (String.IsNullOrWhiteSpace(this.Name))
            {
                yield return "error Name";
            }
        }

        public int Time { get; set; }
    }
}