using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Ps3DiscDumper.Sfb;

public static class SfbReader
{
    public static Sfb Parse(byte[] content)
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        if (content.Length < 200)
            throw new ArgumentException("Data is too small to be a valid SFB structure", nameof(content));

        if (BitConverter.ToInt32(content.AsSpan(0, 4)) != Sfb.Magic)
            throw new ArgumentException("Specified file is not a valid SFB file", nameof(content));

        var result = new Sfb();
        using var stream = new MemoryStream(content, false);
        using var reader = new BinaryReader(stream, Encoding.ASCII);
        reader.ReadInt32(); // magic
        result.VersionMajor = BinaryPrimitives.ReadInt16BigEndian(reader.ReadBytes(2));
        result.VersionMinor = BinaryPrimitives.ReadInt16BigEndian(reader.ReadBytes(2));
        result.Unknown1 = reader.ReadBytes(0x18);
        do
        {
            var keyEntry = new SfbKeyEntry();
            keyEntry.Key = Encoding.ASCII.GetString(reader.ReadBytes(0x10)).TrimEnd('\0');
            if (string.IsNullOrEmpty(keyEntry.Key))
                break;

            keyEntry.ValueOffset = BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4));
            keyEntry.ValueLength = BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4));
            keyEntry.Unknown = reader.ReadInt64();
            result.KeyEntries.Add(keyEntry);
        } while (true);
        foreach (var entry in result.KeyEntries)
        {
            reader.BaseStream.Seek(entry.ValueOffset, SeekOrigin.Begin);
            entry.Value = Encoding.ASCII.GetString(reader.ReadBytes(entry.ValueLength)).TrimEnd('\0');
        }
        return result;
    }
}