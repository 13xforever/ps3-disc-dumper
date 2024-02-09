using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ps3DiscDumper.Utils.MacOS;

[SupportedOSPlatform("macos")]
public static class IOKit
{
    private const string LibraryName = "IOKit.framework/IOKit";

    public static readonly IntPtr MasterPortDefault = IntPtr.Zero;
    public static readonly string BDMediaClass = "IOBDMedia";
    public static readonly IntPtr BDMediaClassCFString = CoreFoundation.__CFStringMakeConstantString(BDMediaClass);
    public static readonly IntPtr BSDNameKey = CoreFoundation.__CFStringMakeConstantString("BSD Name");

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr IOServiceMatching(string name);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint IOServiceGetMatchingServices(IntPtr mainPort, IntPtr matching, out IntPtr existing);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr IOIteratorNext(IntPtr iterator);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr IORegistryEntryCreateCFProperty(IntPtr entry, IntPtr key, IntPtr allocator, uint options);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr IOObjectRelease(IntPtr obj);
}
