using System;
using System.Collections.Generic;

namespace Ps3DiscDumper.Sfb;

public class Sfb
{
    public static readonly int Magic = BitConverter.ToInt32(".SFB"u8);
    public short VersionMajor;
    public short VersionMinor;
    public byte[] Unknown1; // 0x18
    public readonly List<SfbKeyEntry> KeyEntries = new();
}

public class SfbKeyEntry
{
    public string Key; // 0x10
    public int ValueOffset;
    public int ValueLength;
    public long Unknown;
    public string Value;
}