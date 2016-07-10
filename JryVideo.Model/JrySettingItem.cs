using JryVideo.Model.Interfaces;

namespace JryVideo.Model
{
    public class JrySettingItem : IObject
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

        public int Version { get; set; }

        public void CheckError()
        {
        }
    }
}