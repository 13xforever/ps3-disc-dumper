using System;

namespace IrdLibraryClient.Utils
{
    public static class Utils
    {
        public static string Trim(this string str, int maxLength)
        {
            const int minSaneLimit = 4;

            if (maxLength < minSaneLimit)
                throw new ArgumentException("Argument cannot be less than " + minSaneLimit, nameof(maxLength));

            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Length > maxLength)
                return str[..(maxLength - 3)] + "...";

            return str;
        }

        public static string Truncate(this string str, int maxLength)
        {
            if (maxLength < 1)
                throw new ArgumentException("Argument must be positive, but was " + maxLength, nameof(maxLength));

            if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
                return str;

            return str[..maxLength];
        }

        public static string? Sanitize(this string? str)
        {
            return str?.Replace("`", "`\u200d").Replace("@", "@\u200d");
        }

        public static int Clamp(this int amount, int low, int high)
        {
            return Math.Min(high, Math.Max(amount, low));
        }
    }
}
