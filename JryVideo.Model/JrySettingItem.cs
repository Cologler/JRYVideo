using Jasily.Data;

namespace JryVideo.Model
{
    public class JrySettingItem : IJasilyEntity<string>
    {
        public JrySettingItem(string key, string value)
        {
            this.Id = key;
            this.Value = value;
        }

        /// <summary>
        /// key of entity
        /// </summary>
        public string Id { get; set; }

        public string Value { get; set; }
    }
}