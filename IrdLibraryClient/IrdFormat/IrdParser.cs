using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Force.Crc32;

namespace IrdLibraryClient.IrdFormat
{
    public static class IrdParser
    {
        public static Ird Parse(byte[] content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            if (content.Length < 200)
                throw new ArgumentException("Data is too small to be a valid IRD structure", nameof(content));

            if (BitConverter.ToInt32(content, 0) != Ird.Magic)
                using (var compressedStream = new MemoryStream(content, false))
                using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
                using (var decompressedStream = new MemoryStream())
                {
                    gzip.CopyTo(decompressedStream);
                    content = decompressedStream.ToArray();
                }
            if (BitConverter.ToInt32(content, 0) != Ird.Magic)
                throw new FormatException("Not a valid IRD file");

            var result = new Ird();
            using (var stream = new MemoryStream(content, false))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                reader.ReadInt32(); // magic
                result.Version = reader.ReadByte();
                result.ProductCode = Encoding.ASCII.GetString(reader.ReadBytes(9));
                result.TitleLength = reader.ReadByte();
                result.Title = Encoding.UTF8.GetString(reader.ReadBytes(result.TitleLength));
                result.UpdateVersion = Encoding.ASCII.GetString(reader.ReadBytes(4)).Trim();
                result.GameVersion = Encoding.ASCII.GetString(reader.ReadBytes(5)).Trim();
                result.AppVersion = Encoding.ASCII.GetString(reader.ReadBytes(5)).Trim();
                if (result.Version == 7)
                    result.Id = reader.ReadInt32();
                result.HeaderLength = reader.ReadInt32();
                result.Header = reader.ReadBytes(result.HeaderLength);
                result.FooterLength = reader.ReadInt32();
                result.Footer = reader.ReadBytes(result.FooterLength);
                result.RegionCount = reader.ReadByte();
                result.RegionMd5Checksums = new List<byte[]>(result.RegionCount);
                for (var i = 0; i < result.RegionCount; i++)
                    result.RegionMd5Checksums.Add(reader.ReadBytes(16));
                result.FileCount = reader.ReadInt32();
                result.Files = new List<IrdFile>(result.FileCount);
                for (var i = 0; i < result.FileCount; i++)
                {
                    var file = new IrdFile();
                    file.Offset = reader.ReadInt64();
                    file.Md5Checksum = reader.ReadBytes(16);
                    result.Files.Add(file);
                }
                result.Unknown = reader.ReadInt32();
                if (result.Version == 9)
                    result.Pic = reader.ReadBytes(115);
                result.Data1 = reader.ReadBytes(16);
                result.Data2 = reader.ReadBytes(16);
                if (result.Version < 9)
                    result.Pic = reader.ReadBytes(115);
                result.Uid = reader.ReadInt32();
                var dataLength = reader.BaseStream.Position;
                result.Crc32 = reader.ReadUInt32();

                var crc32 = Crc32Algorithm.Compute(content, 0, (int)dataLength);
                if (result.Crc32 != crc32)
                    throw new InvalidDataException($"Corrupted IRD data, expected {result.Crc32:x8}, but was {crc32:x8}");
            }
            return result;
        }
    }
}
