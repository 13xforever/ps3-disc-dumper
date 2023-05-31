using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace IrdLibraryClient.Compression;

public class DecompressedContent : HttpContent
{
    private readonly HttpContent content;
    private readonly ICompressor compressor;

    public DecompressedContent(HttpContent content, ICompressor compressor)
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        if (compressor == null)
            throw new ArgumentNullException(nameof(compressor));

        this.content = content;
        this.compressor = compressor;
        RemoveHeaders();
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
            var decompressedLength = await compressor.DecompressAsync(contentStream, stream).ConfigureAwait(false);
            Headers.ContentLength = decompressedLength;
        }
    }

    private void RemoveHeaders()
    {
        foreach (var header in content.Headers)
            Headers.TryAddWithoutValidation(header.Key, header.Value);
        Headers.ContentEncoding.Clear();
        Headers.ContentLength = null;
    }
}