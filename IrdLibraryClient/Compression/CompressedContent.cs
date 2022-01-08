using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace IrdLibraryClient.Compression
{
    public class CompressedContent : HttpContent
    {
        private readonly HttpContent content;
        private readonly ICompressor compressor;

        public CompressedContent(HttpContent content, ICompressor compressor)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            if (compressor == null)
                throw new ArgumentNullException(nameof(compressor));

            this.content = content;
            this.compressor = compressor;
            AddHeaders();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (content)
            {
                var contentStream = await content.ReadAsStreamAsync().ConfigureAwait(false);
                var compressedLength = await compressor.CompressAsync(contentStream, stream).ConfigureAwait(false);
                Headers.ContentLength = compressedLength;
            }
        }

        private void AddHeaders()
        {
            foreach (var header in content.Headers)
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            Headers.ContentEncoding.Add(compressor.EncodingType);
            Headers.ContentLength = null;
        }
    }
}