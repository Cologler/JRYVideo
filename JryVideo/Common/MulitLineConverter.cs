using System;
using System.Collections.Generic;
using System.Linq;

namespace JryVideo.Common
{
    public class MulitLineConverter : Jasily.Converter<List<string>, string>
    {
        #region Overrides of Converter<List<string>,string>

        public override bool CanConvert(List<string> value) => true;

        public override string Convert(List<string> value)
        {
            return value == null ? string.Empty : value.AsLines();
        }

        public override bool CanConvertBack(string value) => true;

        public override List<string> ConvertBack(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            return value.AsLines()
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .Select(z => z.Trim())
                .Distinct()
                .ToList();
        }

        #endregion
    }
}