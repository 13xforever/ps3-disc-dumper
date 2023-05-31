using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ps3DiscDumper.Sfo;

public class ParamSfo
{
    private ParamSfo() { }

    public string Magic { get; private set; }
    public byte MajorVersion { get; private set; }
    public byte MinorVersion { get; private set; }
    public short Reserved1 { get; private set; }
    public int KeysOffset { get; private set; }
    public int ValuesOffset { get; private set; }
    private int ItemCount { get; set; }
    public List<ParamSfoEntry> Items { get; private set; }

    public static ParamSfo ReadFrom(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        stream.Seek(0, SeekOrigin.Begin);
        var result = new ParamSfo();
        using var reader = new BinaryReader(stream, new UTF8Encoding(false), true);
        result.Magic = new(reader.ReadChars(4));
        if (result.Magic != "\0PSF")
            throw new FormatException("Not a valid SFO file");

        result.MajorVersion = reader.ReadByte();
        result.MinorVersion = reader.ReadByte();
        result.Reserved1 = reader.ReadInt16();
        result.KeysOffset = reader.ReadInt32();
        result.ValuesOffset = reader.ReadInt32();
        result.ItemCount = reader.ReadInt32();
        result.Items = new(result.ItemCount);

        for (var i = 0; i < result.ItemCount; i++)
            result.Items.Add(ParamSfoEntry.Read(reader, result, i));

        return result;
    }

    public void WriteTo(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        var utf8 = new UTF8Encoding(false);
        using var writer = new BinaryWriter(stream, utf8, true);
        writer.Write(utf8.GetBytes(Magic));
        writer.Write(MajorVersion);
        writer.Write(MinorVersion);
        writer.Write(Reserved1);
        KeysOffset = 0x14 + Items.Count * 0x10;
        writer.Write(KeysOffset);
        ValuesOffset = KeysOffset + Items.Sum(i => i.Key.Length + 1);
        if (ValuesOffset % 4 != 0)
            ValuesOffset = (ValuesOffset / 4 + 1) * 4;
        writer.Write(ValuesOffset);
        ItemCount = Items.Count;
        writer.Write(ItemCount);

        int lastKeyOffset = KeysOffset;
        int lastValueOffset = ValuesOffset;
        for (var i = 0; i < Items.Count; i++)
        {
            var entry = Items[i];

            writer.BaseStream.Seek(0x14 + i * 0x10, SeekOrigin.Begin);
            writer.Write((ushort)(lastKeyOffset - KeysOffset));
            writer.Write((ushort)entry.ValueFormat);
            writer.Write(entry.ValueLength);
            writer.Write(entry.ValueMaxLength);
            writer.Write(lastValueOffset - ValuesOffset);

            writer.BaseStream.Seek(lastKeyOffset, SeekOrigin.Begin);
            writer.Write(utf8.GetBytes(entry.Key));
            writer.Write((byte)0);
            lastKeyOffset = (int)writer.BaseStream.Position;

            writer.BaseStream.Seek(lastValueOffset, SeekOrigin.Begin);
            writer.Write(entry.BinaryValue);
            lastValueOffset = (int)writer.BaseStream.Position;
        }
    }
}