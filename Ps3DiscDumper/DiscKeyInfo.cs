using System;
using Ps3DiscDumper.Utils;

namespace Ps3DiscDumper
{
    public class DiscKeyInfo
    {
        public DiscKeyInfo(byte[] encryptedKey, byte[] decryptedKey, string fullPath, KeyType keyType, string keyFileHash)
        {
            if ((encryptedKey == null || encryptedKey.Length == 0) && (decryptedKey == null || decryptedKey.Length == 0))
                throw new ArgumentException("At least one type of disc key must be provided", nameof(encryptedKey));

            if (string.IsNullOrEmpty(keyFileHash))
                throw new ArgumentException("Key file hash is required and can not be empty", nameof(keyFileHash));

            if (decryptedKey == null || decryptedKey.Length == 0)
                DecryptedKey = Decrypter.GetDecryptionKey(encryptedKey);
            else
                DecryptedKey = decryptedKey;
            EncryptedKey = encryptedKey;
            DecryptedKeyId = DecryptedKey.ToHexString();
            FullPath = fullPath;
            KeyType = keyType;
            KeyFileHash = keyFileHash;
        }

        public readonly byte[] EncryptedKey;
        public readonly byte[] DecryptedKey;
        public readonly string FullPath;
        public readonly KeyType KeyType;
        public readonly string DecryptedKeyId;
        public readonly string KeyFileHash;

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(DiscKeyInfo)) return false;
            return Equals((DiscKeyInfo)obj);
        }

        protected bool Equals(DiscKeyInfo other)
        {
            return string.Equals(DecryptedKeyId, other.DecryptedKeyId)
                   && string.Equals(KeyFileHash, other.KeyFileHash)
                   && KeyType == other.KeyType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DecryptedKeyId?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (KeyFileHash?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)KeyType;
                return hashCode;
            }
        }

        public static bool operator ==(DiscKeyInfo left, DiscKeyInfo right) { return Equals(left, right); }
        public static bool operator !=(DiscKeyInfo left, DiscKeyInfo right) { return !Equals(left, right); }
    }

    public enum KeyType
    {
        Ird,
        Redump,
    }
}
