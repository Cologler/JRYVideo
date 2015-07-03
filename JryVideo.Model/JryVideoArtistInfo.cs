using System;
using System.Diagnostics;

namespace JryVideo.Model
{
    public sealed class JryVideoArtistInfo : JryObject, IJasilyLoggerObject<JryVideoArtistInfo>
    {
        public string ArtistName { get; set; }

        public string ArtistId { get; set; }

        public string RoleName { get; set; }

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) 
                return true;

            if (this.ArtistName.IsNullOrWhiteSpace())
            {
                this.Log(JasilyLogger.LoggerMode.Debug, "name can not be empty.");
                return true;
            }

            if (this.ArtistId.IsNullOrWhiteSpace())
            {
                this.Log(JasilyLogger.LoggerMode.Debug, "name can not be empty.");
            }

            return false;
        }
    }
}