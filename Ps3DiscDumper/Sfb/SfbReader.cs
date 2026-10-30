using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Ps3DiscDumper.Sfb;

public static class SfbReader
{
    public static Sfb Parse(byte[] content)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));
        if (content.Length < 200)
            throw new ArgumentException("Data is too small to be a valid SFB structure", nameof(content));
        if (BitConverter.ToInt32(content.AsSpan(0, 4)) != Sfb.Magic)
            throw new ArgumentException("Specified file is not a valid SFB file", nameof(content));

        using var stream = new MemoryStream(content, false);
        using var reader = new BinaryReader(stream, Encoding.ASCII);
        if (reader.ReadInt32() != Sfb.Magic)
            throw new InvalidOperationException("Not an SFB file");
        
        var versionMajor = BinaryPrimitives.ReadInt16BigEndian(reader.ReadBytes(2));
        var versionMinor = BinaryPrimitives.ReadInt16BigEndian(reader.ReadBytes(2));
        var unknown1 = reader.ReadBytes(0x18);
        var result = new Sfb
        {
            VersionMajor = versionMajor,
            VersionMinor = versionMinor,
            Unknown1 = unknown1,
        };
        do
        {
            var key = Encoding.ASCII.GetString(reader.ReadBytes(0x10)).TrimEnd('\0');
            if (key is not {Length: >0})
                break;

            var valueOffset = BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4));
            var valueLength = BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4));
            var unknown = reader.ReadInt64();
            result.KeyEntries.Add(new()
            {
                Key = key,
                ValueOffset =  valueOffset,
                ValueLength = valueLength,
                Unknown =  unknown,
                Value = null!,
            });
        } while (true);
        foreach (var entry in result.KeyEntries)
        {
            reader.BaseStream.Seek(entry.ValueOffset, SeekOrigin.Begin);
            entry.Value = Encoding.ASCII.GetString(reader.ReadBytes(entry.ValueLength)).TrimEnd('\0');
        }
        return result;
    }
}