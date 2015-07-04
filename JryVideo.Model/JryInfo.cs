using System;

namespace JryVideo.Model
{
    public abstract class JryInfo : JryObject
    {
        /// <summary>
        /// before call this, copy Id from entity of info to this's Id.
        /// </summary>
        /// <param name="isForce"></param>
        public override void BuildMetaData(bool isForce = false)
        {
            if (this.Id == null) throw new Exception("can not accept empty id.");
            if (!isForce) throw new Exception("can not rebuild meta data.");

            this.Created = DateTime.UtcNow;
        }
    }
}