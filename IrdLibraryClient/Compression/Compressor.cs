namespace IrdLibraryClient.Compression;

public abstract class Compressor : ICompressor
{
    public abstract string EncodingType { get; }
    public abstract Stream CreateCompressionStream(Stream output);
    public abstract Stream CreateDecompressionStream(Stream input);

    public virtual async Task<long> CompressAsync(Stream source, Stream destination)
    {
        await using (var memStream = new MemoryStream())
        {
            await using (var compressed = CreateCompressionStream(memStream))
                await source.CopyToAsync(compressed).ConfigureAwait(false);
            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(destination).ConfigureAwait(false);
            return memStream.Length;
        }
    }

    public virtual async Task<long> DecompressAsync(Stream source, Stream destination)
    {
        await using (var memStream = new MemoryStream())
        {
            await using (var decompressed = CreateDecompressionStream(source))
                await decompressed.CopyToAsync(memStream).ConfigureAwait(false);
            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(destination).ConfigureAwait(false);
            return memStream.Length;
        }
    }
}