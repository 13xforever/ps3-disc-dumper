using System.Collections.Specialized;

namespace UI.WinForms.Msil
{
    internal static class PatternFormatter
    {
        public static string Format(string pattern, NameValueCollection items)
        {
            if (string.IsNullOrEmpty(pattern))
                return pattern;

            foreach (var k in items.AllKeys)
                pattern = pattern.Replace($"%{k}%", items[k]);
            return pattern;
        }
    }
}
