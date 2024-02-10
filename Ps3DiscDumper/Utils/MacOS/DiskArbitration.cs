using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ps3DiscDumper.Utils.MacOS;

[SupportedOSPlatform("osx")]
public static partial class DiskArbitration
{
    private const string LibraryName = "DiskArbitration.framework/DiskArbitration";

    public static readonly IntPtr DescriptionMediaKindKey = CoreFoundation.__CFStringMakeConstantString("DAMediaKind");

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr DASessionCreate(IntPtr allocator);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void DARegisterDiskAppearedCallback(IntPtr session, IntPtr match, IntPtr callback, IntPtr context);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void DARegisterDiskDisappearedCallback(IntPtr session, IntPtr match, IntPtr callback, IntPtr context);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr DAUnregisterCallback(IntPtr session, IntPtr callback, IntPtr context);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr DASessionScheduleWithRunLoop(IntPtr session, IntPtr runLoop, IntPtr runLoopMode);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr DASessionUnscheduleFromRunLoop(IntPtr session, IntPtr runLoop, IntPtr runloopMode);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr DADiskGetBSDName(IntPtr disk);
}
