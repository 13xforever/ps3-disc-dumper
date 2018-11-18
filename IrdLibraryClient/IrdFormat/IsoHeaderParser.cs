using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DiscUtils.Iso9660;

namespace IrdLibraryClient.IrdFormat
{
    public static class IsoHeaderParser
    {
        public static List<string> GetFilenames(this Ird ird)
        {
            using (var decompressedStream = new MemoryStream())
            {
                using (var compressedStream = new MemoryStream(ird.Header, false))
                using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
                    gzip.CopyTo(decompressedStream);

                decompressedStream.Seek(0, SeekOrigin.Begin);
                var reader = new CDReader(decompressedStream, true, true);
                return reader.GetFiles(reader.Root.FullName, "*.*", SearchOption.AllDirectories).Distinct().Select(n => n.TrimStart('\\').Replace('\\', '/')).ToList();
            }
        }
    }
}