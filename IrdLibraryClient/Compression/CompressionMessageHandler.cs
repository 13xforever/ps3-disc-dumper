using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IrdLibraryClient.Compression;

public class CompressionMessageHandler : DelegatingHandler
{
    public ICollection<ICompressor> Compressors { get; }
    public static readonly string PostCompressionFlag = "X-Set-Content-Encoding";
    public static readonly string[] DefaultContentEncodings = { "gzip", "deflate" };
    public static readonly string DefaultAcceptEncodings = "gzip, deflate";

    private bool isServer;
    private bool isClient => !isServer;

    public CompressionMessageHandler(bool isServer = false)
    {
        this.isServer = isServer;
        Compressors = new ICompressor[]
        {
            new GZipCompressor(),
            new DeflateCompressor(),
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (isServer && request.Content?.Headers?.ContentEncoding != null)
        {
            var encoding = request.Content.Headers.ContentEncoding.FirstOrDefault();
            if (encoding != null)
            {
                var compressor = Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.InvariantCultureIgnoreCase));
                if (compressor != null)
                    request.Content = new DecompressedContent(request.Content, compressor);
            }
        }
        if (isClient && (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put) && request.Content != null && request.Headers != null && request.Headers.Contains(PostCompressionFlag))
        {
            var encoding = request.Headers.GetValues(PostCompressionFlag).FirstOrDefault();
            if (encoding != null)
            {
                var compressor = Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.InvariantCultureIgnoreCase));
                if (compressor != null)
                    request.Content = new CompressedContent(request.Content, compressor);
            }
        }
        request.Headers?.Remove(PostCompressionFlag);
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (isClient && response.Content?.Headers?.ContentEncoding != null)
        {
            var encoding = response.Content.Headers.ContentEncoding.FirstOrDefault();
            if (encoding != null)
            {
                var compressor = Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.InvariantCultureIgnoreCase));
                if (compressor != null)
                    response.Content = new DecompressedContent(response.Content, compressor);
            }
        }
        if (isServer && response.Content != null && request.Headers?.AcceptEncoding != null)
        {
            var encoding = request.Headers.AcceptEncoding.FirstOrDefault();
            if (encoding == null)
                return response;

            var compressor = Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding.Value, StringComparison.InvariantCultureIgnoreCase));
            if (compressor == null)
                return response;

            response.Content = new CompressedContent(response.Content, compressor);
        }
        return response;
    }
}