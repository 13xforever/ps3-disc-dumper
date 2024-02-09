using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ps3DiscDumper.Utils.MacOS;

[SupportedOSPlatform("macos")]
public static class DiskArbitration
{
    private const string LibraryName = "DiskArbitration.framework/DiskArbitration";

    public static readonly IntPtr DescriptionMediaKindKey = CoreFoundation.__CFStringMakeConstantString("DAMediaKind");

    public delegate void DADiskAppearedCallback(IntPtr disk, IntPtr context);
    public delegate void DADiskDisappearedCallback(IntPtr disk, IntPtr context);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr DASessionCreate(IntPtr allocator);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void DARegisterDiskAppearedCallback(IntPtr session, IntPtr match, IntPtr callback, IntPtr context);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void DARegisterDiskDisappearedCallback(IntPtr session, IntPtr match, IntPtr callback, IntPtr context);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr DAUnregisterCallback(IntPtr session, IntPtr callback, IntPtr context);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr DASessionScheduleWithRunLoop(IntPtr session, IntPtr runLoop, IntPtr runLoopMode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr DASessionUnscheduleFromRunLoop(IntPtr session, IntPtr runLoop, IntPtr runloopMode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr DADiskGetBSDName(IntPtr disk);
}
