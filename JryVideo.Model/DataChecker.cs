using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace JryVideo.Model
{
    public static class DataChecker
    {
        private static string Format(string path, int line)
            => $"[{path}] ({line})";

        public static void NotNull<T>([NotNull] T value,
            [CallerFilePath] string path = null, [CallerLineNumber] int line = 0)
        {
            if (value == null)
            {
                var format = Format(path, line);
                throw new DataCheckerException($"{format} value cannot be null.");
            }
        }

        public static void NotEmpty<T>([NotNull] List<T> list,
            [CallerFilePath] string path = null, [CallerLineNumber] int line = 0)
        {
            if (list.Count == 0)
            {
                var format = Format(path, line);
                throw new DataCheckerException($"{format} item of list connot be zero.");
            }
        }

        public static void NotEmpty([NotNull] string value,
            [CallerFilePath] string path = null, [CallerLineNumber] int line = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var format = Format(path, line);
                throw new DataCheckerException($"{format} value cannot be null or whiteSpace.");
            }
        }

        public static void True(bool value,
            [CallerFilePath] string path = null, [CallerLineNumber] int line = 0)
        {
            if (!value)
            {
                var format = Format(path, line);
                throw new DataCheckerException($"{format} value cannot be null or whiteSpace.");
            }
        }
    }
}