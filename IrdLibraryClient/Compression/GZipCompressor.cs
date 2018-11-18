using System.IO;
using System.IO.Compression;

namespace IrdLibraryClient.Compression
{
    public class GZipCompressor : Compressor
    {
        public override string EncodingType => "gzip";

        public override Stream CreateCompressionStream(Stream output)
        {
            return new GZipStream(output, CompressionMode.Compress, true);
        }

        public override Stream CreateDecompressionStream(Stream input)
        {
            return new GZipStream(input, CompressionMode.Decompress, true);
        }
    }
}