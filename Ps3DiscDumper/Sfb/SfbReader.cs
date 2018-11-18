using System;
using System.IO;
using System.Text;
using BitConverter;

namespace Ps3DiscDumper.Sfb
{
    public static class SfbReader
    {
        public static Ps3DiscDumper.Sfb.Sfb Parse(byte[] content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            if (content.Length < 200)
                throw new ArgumentException("Data is too small to be a valid SFB structure", nameof(content));

            if (EndianBitConverter.BigEndian.ToInt32(content, 0) != Ps3DiscDumper.Sfb.Sfb.Magic)
                throw new ArgumentException("Specified file is not a valid SFB file", nameof(content));

            var result = new Ps3DiscDumper.Sfb.Sfb();
            using (var stream = new MemoryStream(content, false))
            using (var reader = new BinaryReader(stream, Encoding.ASCII))
            {
                reader.ReadInt32(); // magic
                result.VersionMajor = EndianBitConverter.BigEndian.ToInt16(reader.ReadBytes(2), 0);
                result.VersionMinor = EndianBitConverter.BigEndian.ToInt16(reader.ReadBytes(2), 0);
                result.Unknown1 = reader.ReadBytes(0x18);
                do
                {
                    var keyEntry = new SfbKeyEntry();
                    keyEntry.Key = Encoding.ASCII.GetString(reader.ReadBytes(0x10)).TrimEnd('\0');
                    if (string.IsNullOrEmpty(keyEntry.Key))
                        break;

                    keyEntry.ValueOffset = EndianBitConverter.BigEndian.ToInt32(reader.ReadBytes(4), 0);
                    keyEntry.ValueLength = EndianBitConverter.BigEndian.ToInt32(reader.ReadBytes(4), 0);
                    keyEntry.Unknown = reader.ReadInt64();
                    result.KeyEntries.Add(keyEntry);
                } while (true);
                foreach (var entry in result.KeyEntries)
                {
                    reader.BaseStream.Seek(entry.ValueOffset, SeekOrigin.Begin);
                    entry.Value = Encoding.ASCII.GetString(reader.ReadBytes(entry.ValueLength)).TrimEnd('\0');
                }
            }
            return result;
        }
    }
}
