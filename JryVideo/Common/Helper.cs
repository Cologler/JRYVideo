using System.Linq;
using System.Text.RegularExpressions;

namespace JryVideo.Common
{
    public static class Helper
    {
        private static readonly Regex DoubanIdRegex = new Regex(@"(subject|celebrity)/(\d+)");
        private static readonly Regex ImdbIdRegex = new Regex(@"((tt|nm)\d+)");

        public static string TryGetDoubanId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (value.All(char.IsDigit)) return value;
            var match = DoubanIdRegex.Match(value);
            if (match.Success) return match.Groups[2].Value;
            return null;
        }

        public static string TryGetImdbId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var match = ImdbIdRegex.Match(value);
            if (match.Success) return match.Groups[1].Value;
            return null;
        }
    }
}