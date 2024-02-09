using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace Ps3DiscDumper.Utils.MacOS;

[SupportedOSPlatform("macos")]
public static class CoreFoundation
{
    private const string ApplicationServicesLibraryName = "ApplicationServices.framework/ApplicationServices";
    private const string LibraryName = "CoreFoundation.framework/CoreFoundation";
    private const string LibraryPath = "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation";

    public static readonly uint KernSuccess = 0;
    public static readonly uint StringEncodingASCII = 0x0600;
    public static readonly IntPtr RunLoopDefaultMode = CoreFoundation.__CFStringMakeConstantString("kCFRunLoopDefaultMode");

    private static IntPtr Library;
    public static IntPtr TypeDictionaryKeyCallBacks;
    public static IntPtr TypeDictionaryValueCallBacks;

    static CoreFoundation()
    {
        Library = dlopen(LibraryPath, 0);
        TypeDictionaryKeyCallBacks = dlsym(Library, "kCFTypeDictionaryKeyCallBacks");
        TypeDictionaryValueCallBacks = dlsym(Library, "kCFTypeDictionaryValueCallBacks");
    }

    [DllImport("libdl", ExactSpelling = true)]
    public static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libdl", ExactSpelling = true)]
    public static extern IntPtr dlsym(IntPtr handle, string symbol);

    [DllImport(ApplicationServicesLibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr __CFStringMakeConstantString(string cStr);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CFAllocatorGetDefault();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void CFRelease(IntPtr ptr);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CFRunLoopGetCurrent();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void CFRunLoopRun();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void CFRunLoopStop(IntPtr runLoop);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CFDictionaryCreate(IntPtr allocator, IntPtr[] keys, IntPtr[] values, nint numValues, IntPtr keyCallbacks, IntPtr valueCallbacks);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool CFStringGetCString(IntPtr theString, StringBuilder buffer, long bufferSize, uint encoding);
}
