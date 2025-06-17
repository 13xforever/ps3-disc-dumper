using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ps3DiscDumper.Utils.MacOS;

[SupportedOSPlatform("osx")]
public static partial class IOKit
{
    private const string LibraryName = "/System/Library/Frameworks/IOKit.framework/Versions/Current/IOKit";

    public static readonly IntPtr MasterPortDefault = IntPtr.Zero;
    public const string BdMediaClass = "IOBDMedia";
    public static readonly IntPtr BdMediaClassCfString = CoreFoundation.__CFStringMakeConstantString(BdMediaClass);
    public static readonly IntPtr BsdNameKey = CoreFoundation.__CFStringMakeConstantString("BSD Name");

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr IOServiceMatching(string name);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial uint IOServiceGetMatchingServices(IntPtr mainPort, IntPtr matching, out IntPtr existing);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr IOIteratorNext(IntPtr iterator);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr IORegistryEntryCreateCFProperty(IntPtr entry, IntPtr key, IntPtr allocator, uint options);

    [LibraryImport(LibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr IOObjectRelease(IntPtr obj);
}
