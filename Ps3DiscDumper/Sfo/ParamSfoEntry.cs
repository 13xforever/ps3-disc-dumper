using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Ps3DiscDumper.Sfo;

[DebuggerDisplay("{StringValue}", Name = "{Key}")]
public class ParamSfoEntry
{
    private ParamSfoEntry() { }

    private ushort KeyOffset { get; set; }
    public EntryFormat ValueFormat { get; private init; }
    public int ValueLength { get; private set; }
    public int ValueMaxLength { get; private init; }
    private int ValueOffset { get; init; }
    public string Key { get; init; } = null!;
    public byte[] BinaryValue { get; init; } = null!;

    private static readonly char[] Whitespace = ['\r', '\n', '\0', '\t', ' '];

    public string StringValue
    {
        get
        {
            if (ValueFormat is EntryFormat.Utf8 or EntryFormat.Utf8Null)
            {
                var result = Encoding.UTF8.GetString(BinaryValue, 0, ValueLength).Trim(Whitespace);
                if (Environment.NewLine != "\n")
                    result = result.Replace("\n", Environment.NewLine);
                return result;
            }

            throw new InvalidOperationException("Current value format is not a string");
        }
        set
        {
            if (ValueFormat is not EntryFormat.Utf8 and not EntryFormat.Utf8Null)
                throw new InvalidOperationException("Current value format is not a string");

            var saneValue = value.Trim(Whitespace);
            if (Environment.NewLine != "\n")
                saneValue = saneValue.Replace(Environment.NewLine, "\n");

            var tmp = new UTF8Encoding(false).GetBytes(saneValue);
            var newLen = tmp.Length;
            if (ValueFormat is EntryFormat.Utf8Null)
                newLen++;
            if (newLen > ValueMaxLength)
                throw new FormatException($"Current value length is {newLen} bytes, but maximum value length is {ValueMaxLength} bytes");


            Buffer.BlockCopy(tmp, 0, BinaryValue, 0, tmp.Length);
            if (tmp.Length < BinaryValue.Length)
                Array.Clear(BinaryValue, tmp.Length, BinaryValue.Length - tmp.Length);
            ValueLength = newLen;
        }
    }

    public static ParamSfoEntry Read(BinaryReader reader, ParamSfo paramSfo, int itemNumber)
    {
        const int indexOffset = 0x14;
        const int indexEntryLength = 0x10;
        reader.BaseStream.Seek(indexOffset + indexEntryLength * itemNumber, SeekOrigin.Begin);
        var keyOffset = reader.ReadUInt16();
        var valueFormat = (EntryFormat)reader.ReadUInt16();
        var valueLength = reader.ReadInt32();
        var valueMaxLength = reader.ReadInt32();
        var valueOffset = reader.ReadInt32();

        reader.BaseStream.Seek(paramSfo.KeysOffset + keyOffset, SeekOrigin.Begin);
        byte tmp;
        var sb = new StringBuilder(32);
        while ((tmp = reader.ReadByte()) != 0)
            sb.Append((char)tmp);
        var key = sb.ToString();

        reader.BaseStream.Seek(paramSfo.ValuesOffset + valueOffset, SeekOrigin.Begin);
        var binaryValue = reader.ReadBytes(valueMaxLength);

        return new()
        {
            KeyOffset = keyOffset,
            ValueFormat = valueFormat,
            ValueLength = valueLength,
            ValueMaxLength = valueMaxLength,
            ValueOffset = valueOffset,
            Key = key,
            BinaryValue = binaryValue,
        };
    }
}