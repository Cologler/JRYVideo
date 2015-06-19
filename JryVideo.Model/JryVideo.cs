using System;
using System.Collections.Generic;
using System.Linq;

namespace JryVideo.Model
{
    public sealed class JryVideo : JryObject, IInitializable<JryVideo>
    {
        public string Type { get; set; }

        public int Year { get; set; }

        public int Index { get; set; }

        public List<string> Names { get; set; }

        public List<JryEntity> Entities { get; set; }

        public string DoubanId { get; set; }

        public string ImdbId { get; set; }

        public List<string> Tags { get; set; }

        public Guid CoverId { get; set; }

        public JryVideo InitializeInstance(JryVideo obj)
        {
            obj.CoverId = Guid.Empty;
            obj.Names = new List<string>();
            obj.Entities = new List<JryEntity>();
            obj.Tags = new List<string>();

            return base.InitializeInstance(obj);
        }

        public override IEnumerable<string> CheckError()
        {
            foreach (var error in base.CheckError())
            {
                yield return error;
            }

            if (this.Names == null || this.Tags == null || this.Entities == null)
            {
                yield return "error InitializeInstance()";
            }

            if (String.IsNullOrWhiteSpace(this.Type))
            {
                yield return "error Type";
            }

            if (this.Year < 1900 || this.Year > 2100)
            {
                yield return "error Year";
            }

            if (this.Index < 1 || this.Index > 100)
            {
                yield return "error Index";
            }
        }
    }
}