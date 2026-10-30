using System;
using Ps3DiscDumper.Utils;

namespace Ps3DiscDumper;

public class DiscKeyInfo
{
    public DiscKeyInfo(byte[] key, bool keyIsEncrypted, string fullPath, KeyType keyType, string keyFileHash)
    {
        Key = (key, keyIsEncrypted) switch
        {
            ({ Length: >0 } k, true) => Decrypter.DecryptDiscKey(k),
            ({ Length: >0 } k, false) => k,
            _ => throw new ArgumentException("At least one type of disc key must be provided", nameof(key))
        };

        this.keyFileHash = keyFileHash switch
        {
            { Length: >0 } => keyFileHash,
            _ => throw new ArgumentException("Key file hash is required and can not be empty", nameof(keyFileHash))
        };

        KeyId = Key.ToHexString();
        FullPath = fullPath;
        KeyType = keyType;
    }

    public readonly byte[] Key;
    public readonly string FullPath;
    public readonly KeyType KeyType;
    public readonly string KeyId;
    private readonly string keyFileHash;

    public override bool Equals(object? obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != typeof(DiscKeyInfo)) return false;
        return Equals((DiscKeyInfo)obj);
    }

    protected bool Equals(DiscKeyInfo other)
        => string.Equals(KeyId, other.KeyId)
           && string.Equals(keyFileHash, other.keyFileHash)
           && KeyType == other.KeyType;

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = KeyId.GetHashCode();
            hashCode = (hashCode * 397) ^ keyFileHash.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)KeyType;
            return hashCode;
        }
    }

    public static bool operator ==(DiscKeyInfo left, DiscKeyInfo right) { return Equals(left, right); }
    public static bool operator !=(DiscKeyInfo left, DiscKeyInfo right) { return !Equals(left, right); }
}