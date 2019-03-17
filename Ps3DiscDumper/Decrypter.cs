using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using IrdLibraryClient;
using Ps3DiscDumper.Utils;

namespace Ps3DiscDumper
{
    public class Decrypter : Stream, IDisposable
    {
        private static readonly byte[] Secret = "380bcf0b53455b3c7817ab4fa3ba90ed".ToByteArray();
        private static readonly byte[] IV = "69474772af6fdab342743aefaa186287".ToByteArray();

        private Stream inputStream;
        private Stream discStream;
        private byte[] decryptionKey;
        private readonly int sectorSize;
        private readonly MD5 md5;
        private readonly Aes aes;
        private byte[] bufferedSector, tmpSector, hash = null;
        private readonly List<(int start, int end)> unprotectedSectorRanges;

        public static byte[] GetDecryptionKey(byte[] data1)
        {
            using (var aes = Aes.Create())
            {
                aes.BlockSize = data1.Length * 8;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                using (var transform = aes.CreateEncryptor(Secret, IV))
                    return transform.TransformFinalBlock(data1, 0, data1.Length);
            }
        }

        public static byte[] GetSectorIV(long sectorNumber)
        {
            var result = new byte[16];
            for (var i = 15; i > 7; i--)
            {
                result[i] = (byte)(sectorNumber & 0xFF);
                sectorNumber >>= 8;
            }
            return result;
        }

        public static byte[] DecryptSector(byte[] decryptionKey, byte[] sector, byte[] sectorIV)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                using (var aesTransform = aes.CreateDecryptor(decryptionKey, sectorIV))
                    return aesTransform.TransformFinalBlock(sector, 0, sector.Length);
            }
        }

        public Decrypter(Stream fileStream, Stream discStream, byte[] decryptionKey, long startSector, int sectorSize, List<(int start, int end)> unprotectedSectorRanges)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            if (decryptionKey == null)
                throw new ArgumentNullException(nameof(decryptionKey));

            if (decryptionKey.Length != 128 / 8 && decryptionKey.Length != 256 / 8)
                throw new ArgumentException($"Unsupported decryption key size of {decryptionKey.Length * 8} bits. Expected 128 or 256 bit key");

            if (!fileStream.CanRead)
                throw new ArgumentException("Input stream should be readable", nameof(fileStream));

            if (discStream == null)
                throw new ArgumentException("Physical device is needed for proper decryption");

            if (sectorSize == 0)
                throw new ArgumentException("Sector size cannot be 0", nameof(sectorSize));

            inputStream = fileStream;
            this.discStream = discStream;
            this.decryptionKey = decryptionKey;
            md5 = MD5.Create();
            aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            this.sectorSize = sectorSize;
            bufferedSector = new byte[sectorSize];
            tmpSector = new byte[sectorSize];
            this.unprotectedSectorRanges = unprotectedSectorRanges;
            SectorPosition = startSector;
        }

        public override int Read( byte[] buffer, int offset, int count)
        {
            if (Position == inputStream.Length)
                return 0;

            var positionInSector = Position % sectorSize;
            var resultCount = 0;
            if (positionInSector > 0)
            {
                var len = (int)Math.Min(Math.Min(count, sectorSize - positionInSector), inputStream.Position - Position);
                md5.TransformBlock(bufferedSector, (int)positionInSector, len, buffer, offset);
                offset += len;
                count -= len;
                resultCount += len;
                Position += len;
                if (Position % sectorSize == 0)
                    SectorPosition++;
            }
            if (Position == inputStream.Length)
                return resultCount;

            int readCount;
            do
            {
                readCount = inputStream.ReadExact(tmpSector, 0, sectorSize);
                if (readCount < sectorSize)
                    Array.Clear(tmpSector, readCount, sectorSize - readCount);
                var decryptedSector = tmpSector;
                if (IsEncrypted(SectorPosition))
                {
                    WasEncrypted = true;
                    if (readCount % 16 != 0)
                    {
                        Log.Debug($"Block has only {(readCount % 16) * 8} bits of data, reading raw sector...");
                        discStream.Seek(SectorPosition * sectorSize, SeekOrigin.Begin);
                        var newTmpSector = new byte[sectorSize];
                        discStream.ReadExact(newTmpSector, 0, sectorSize);
                        if (!newTmpSector.Take(readCount).SequenceEqual(tmpSector.Take(readCount)))
                            Log.Warn($"Filesystem data and raw data do not match for sector 0x{SectorPosition:x8}");
                        tmpSector = newTmpSector;
                    }
                    using (var aesTransform = aes.CreateDecryptor(decryptionKey, GetSectorIV(SectorPosition)))
                        decryptedSector = aesTransform.TransformFinalBlock(tmpSector, 0, sectorSize);
                }
                else
                    WasUnprotected = true;
                if (count >= readCount)
                {
                    md5.TransformBlock(decryptedSector, 0, readCount, buffer, offset);
                    offset += readCount;
                    count -= readCount;
                    resultCount += readCount;
                    Position += readCount;
                    SectorPosition++;
                }
                else // partial sector read
                {
                    Buffer.BlockCopy(decryptedSector, 0, bufferedSector, 0, sectorSize);
                    md5.TransformBlock(decryptedSector, 0, count, buffer, offset);
                    offset += count;
                    count = 0;
                    resultCount += count;
                    Position += count;
                }
            } while (count > 0 && readCount == sectorSize);
            return resultCount;
        }

        public byte[] GetMd5()
        {
            if (hash == null)
            {
                md5.TransformFinalBlock(tmpSector, 0, 0);
                hash = md5.Hash;
            }
            return hash;
        }

        private bool IsEncrypted(long sector)
        {
            var result = !unprotectedSectorRanges.Any(r => r.start <= sector && sector <= r.end);
            if (TraceSectors)
                Log.Trace($"{sector:x8}: {(result ? "e" : "")}");
            return result;
        }

        void IDisposable.Dispose() { md5?.Dispose(); }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
        public override void Flush() => throw new NotImplementedException();
        public override void SetLength(long value) => throw new NotImplementedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => inputStream.Length;
        public override long Position { get; set; }
        public long SectorPosition { get; private set; }
        public bool WasEncrypted { get; private set; }
        public bool WasUnprotected { get; private set; }
        public bool LastBlockCorrupted { get; private set; }
        public bool TraceSectors { get; set; }
    }
}