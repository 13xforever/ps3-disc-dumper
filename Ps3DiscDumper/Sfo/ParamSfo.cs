using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ps3DiscDumper.Sfo;

public class ParamSfo
{
    private ParamSfo() { }

    private static readonly int Magic = BinaryPrimitives.ReadInt32LittleEndian("\0PSF"u8);
    public byte MajorVersion { get; private init; }
    public byte MinorVersion { get; private init; }
    public short Reserved1 { get; private init; }
    public int KeysOffset { get; private init; }
    public int ValuesOffset { get; private init; }
    private int ItemCount { get; init; }
    public List<ParamSfoEntry> Items { get; private init; } = null!;

    public static ParamSfo ReadFrom(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new BinaryReader(stream, new UTF8Encoding(false), true);
        if (reader.ReadInt32() != Magic)
            throw new FormatException("Not a valid SFO file");

        var majorVersion = reader.ReadByte();
        var minorVersion = reader.ReadByte();
        var reserved1 = reader.ReadInt16();
        var keysOffset = reader.ReadInt32();
        var valuesOffset = reader.ReadInt32();
        var itemCount = reader.ReadInt32();
        var result = new ParamSfo
        {
            MajorVersion = majorVersion,
            MinorVersion = minorVersion,
            Reserved1 = reserved1,
            KeysOffset = keysOffset,
            ValuesOffset = valuesOffset,
            ItemCount = itemCount,
            Items = new(itemCount),
        };
        for (var i = 0; i < itemCount; i++)
            result.Items.Add(ParamSfoEntry.Read(reader, result, i));
        return result;
    }
}