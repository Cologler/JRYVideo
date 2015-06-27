using System;
using System.Collections.Generic;

namespace JryVideo.Model
{
    public sealed class JryVideo : JryObject
    {
        public JryVideo()
        {
            this.Entities = new List<JryEntity>();
        }

        public List<JryEntity> Entities { get; set; }

        public override IEnumerable<JryInvalidError> CheckError()
        {
            foreach (var error in base.CheckError())
            {
                yield return error;
            }

            if (this.Entities == null)
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Guid.NewGuid().ToString().ToUpper()
        /// </summary>
        /// <returns></returns>
        protected override string BuildId()
        {
            return this.Id;
        }

        public static JryVideo Build(JryVideoInfo info)
        {
            return new JryVideo()
            {
                Id = info.Id
            };
        }
    }
}