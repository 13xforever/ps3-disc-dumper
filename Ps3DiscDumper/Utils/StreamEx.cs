using System.IO;

namespace Ps3DiscDumper.Utils;

public static class StreamEx
{
    public static int ReadExact(this Stream input, byte[] buffer, int offset, int count)
    {
        var result = 0;
        int read;
        do
        {
            read = input.Read(buffer, offset, count);
            result += read;
            offset += read;
            count -= read;
        } while (count > 0 && read > 0);
        return result;
    }
}