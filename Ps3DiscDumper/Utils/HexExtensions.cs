using System;
using System.Globalization;
using System.Text;

namespace Ps3DiscDumper.Utils;

public static class HexExtensions
{
    public static byte[] ToByteArray(this string hexString)
    {
        if (hexString == null)
            return null;

        if (hexString.Length == 0)
            return new byte[0];

        if (hexString.Length % 2 != 0)
            throw new FormatException("Not a valid hex string");

        var result = new byte[hexString.Length / 2];
        for (int i = 0; i < hexString.Length; i += 2)
            result[i / 2] = byte.Parse(hexString.Substring(i, 2), NumberStyles.HexNumber);
        return result;
    }

    public static string ToHexString(this byte[] bytes)
    {
        if (bytes == null)
            return null;

        if (bytes.Length == 0)
            return "";

        var result = new StringBuilder(bytes.Length*2);
        foreach (var b in bytes)
            result.Append(b.ToString("x2"));
        return result.ToString();
    }
}