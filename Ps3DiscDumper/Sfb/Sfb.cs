using System.Buffers.Binary;
using System.Collections.Generic;

namespace Ps3DiscDumper.Sfb;

public class Sfb
{
    public static readonly int Magic = BinaryPrimitives.ReadInt32LittleEndian(".SFB"u8);
    public short VersionMajor;
    public short VersionMinor;
    public required byte[] Unknown1; // 0x18
    public readonly List<SfbKeyEntry> KeyEntries = [];
}

public class SfbKeyEntry
{
    public required string Key; // 0x10
    public int ValueOffset;
    public int ValueLength;
    public long Unknown;
    public required string Value;
}