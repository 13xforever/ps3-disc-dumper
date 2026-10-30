using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Ps3DiscDumper.Utils;

public static class HexExtensions
{
    public static byte[] ToByteArray(this string hexString)
    {
        if (hexString is "")
            return [];

        if (hexString.Length % 2 is 1)
            throw new FormatException("Not a valid hex string");

        var result = new byte[hexString.Length / 2];
        for (var i = 0; i < hexString.Length; i += 2)
            result[i / 2] = byte.Parse(hexString.Substring(i, 2), NumberStyles.HexNumber);
        return result;
    }

    [return: NotNullIfNotNull("bytes")]
    public static string? ToHexString(this byte[]? bytes)
    {
        switch (bytes)
        {
            case null: return null;
            case []: return "";
        }

        var result = new StringBuilder(bytes.Length*2);
        foreach (var b in bytes)
            result.Append(b.ToString("x2"));
        return result.ToString();
    }
}