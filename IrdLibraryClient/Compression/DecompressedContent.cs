using System.Net;

namespace IrdLibraryClient.Compression;

public class DecompressedContent : HttpContent
{
    private readonly HttpContent content;
    private readonly ICompressor compressor;

    public DecompressedContent(HttpContent content, ICompressor compressor)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(compressor);

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
        ArgumentNullException.ThrowIfNull(stream);

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