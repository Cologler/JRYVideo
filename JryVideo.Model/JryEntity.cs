using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Runtime.Remoting.Messaging;

namespace JryVideo.Model
{
    public sealed class JryEntity : JryObject
    {
        public JryEntity()
        {
            this.Fansubs = new List<string>();
            this.Formats = new List<JryFormat>();
            this.SubTitleLanguages = new List<string>();
            this.Tags = new List<string>();
            this.TrackLanguages = new List<string>();
        }

        public List<JryFormat> Formats { get; set; }

        public List<string> Tags { get; set; }

        public List<string> Fansubs { get; set; }

        public List<string> SubTitleLanguages { get; set; }

        public List<string> TrackLanguages { get; set; }

        public string Resolution { get; set; }

        public string FilmSource { get; set; }

        public string Extension { get; set; }

        public override IEnumerable<string> CheckError()
        {
            foreach (var error in base.CheckError())
            {
                yield return error;
            }

            if (this.Fansubs == null || this.Formats == null || this.SubTitleLanguages == null ||
                this.Tags == null || this.TrackLanguages == null)
            {
                yield return "error InitializeInstance()";
            }
        }
    }
}