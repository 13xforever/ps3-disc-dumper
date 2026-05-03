using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace Ps3DiscDumper.Utils.MacOS;

[SupportedOSPlatform("osx")]
public static partial class MacOSInterop
{
    [DllImport("libc", EntryPoint = "statfs", SetLastError = true)]
    private static unsafe extern int statfs(string path, StatFs* buf);

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct StatFs
    {
        public uint f_bsize;
        public int f_iosize;
        public ulong f_blocks;
        public ulong f_bfree;
        public ulong f_bavail;
        public ulong f_files;
        public ulong f_ffree;
        public FsId f_fsid;
        public uint f_owner;
        public uint f_type;
        public uint f_flags;
        public uint f_fssubtype;
        public fixed byte f_fstypename[16];
        public fixed byte f_mntonname[1024];
        public fixed byte f_mntfromname[1024];
        public fixed uint f_reserved[8];
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FsId
    {
        public int val0;
        public int val1;
    }

    public static string? GetDeviceFromMountPoint(string path)
    {
        unsafe
        {
            StatFs buf;
            if (statfs(path, &buf) != 0)
                return null;

            var devicePath = Encoding.UTF8.GetString(buf.f_mntfromname, 1024).TrimEnd('\0');
            if (string.IsNullOrEmpty(devicePath))
                return null;

            // Convert /dev/diskX or /dev/diskXsY to /dev/rdiskX
            if (devicePath.StartsWith("/dev/disk"))
            {
                var rawDevice = "/dev/r" + devicePath[5..]; // skip "/dev/", prepend "/dev/r"
                // Remove slice suffix like s1, s2 if present
                var lastS = rawDevice.LastIndexOf('s');
                if (lastS > "/dev/rdisk".Length && int.TryParse(rawDevice[(lastS + 1)..], out _))
                    rawDevice = rawDevice[..lastS];
                return rawDevice;
            }
            return devicePath;
        }
    }
}
