using System.Buffers.Binary;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class BitFiddlingTests
{
    [Test]
    public void StringToInt()
    {
        Assert.That(BinaryPrimitives.ReadInt32BigEndian(".SFB"u8), Is.EqualTo(0x2e534642));
    }
}