using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace Ps3DiscDumper.Utils.MacOS;

[SupportedOSPlatform("osx")]
public static partial class CoreFoundation
{
    private const string AsLibraryName = "/System/Library/Frameworks/ApplicationServices.framework/Versions/Current/ApplicationServices";
    private const string CfLibraryName = "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation";

    public const uint KernSuccess = 0;
    public const uint StringEncodingAscii = 0x0600;
    public static readonly IntPtr RunLoopDefaultMode = __CFStringMakeConstantString("kCFRunLoopDefaultMode");
    public static readonly IntPtr TypeDictionaryKeyCallBacks;
    public static readonly IntPtr TypeDictionaryValueCallBacks;

    static CoreFoundation()
    {
        var library = DlOpen(CfLibraryName, 0);
        TypeDictionaryKeyCallBacks = DlSym(library, "kCFTypeDictionaryKeyCallBacks");
        TypeDictionaryValueCallBacks = DlSym(library, "kCFTypeDictionaryValueCallBacks");
    }

    [LibraryImport("libdl", EntryPoint = "dlopen", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr DlOpen(string filename, int flags);

    [LibraryImport("libdl", EntryPoint = "dlsym", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr DlSym(IntPtr handle, string symbol);

    [LibraryImport(AsLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr __CFStringMakeConstantString(string cStr);

    [LibraryImport(CfLibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr CFAllocatorGetDefault();

    [LibraryImport(CfLibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void CFRelease(IntPtr ptr);

    [LibraryImport(CfLibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr CFRunLoopGetCurrent();

    [LibraryImport(CfLibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void CFRunLoopRun();

    [LibraryImport(CfLibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void CFRunLoopStop(IntPtr runLoop);

    [LibraryImport(CfLibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr CFDictionaryCreate(IntPtr allocator, IntPtr[] keys, IntPtr[] values, nint numValues, IntPtr keyCallbacks, IntPtr valueCallbacks);

    //StringBuilder is not supported for codegen with LibraryImport
    [LibraryImport(CfLibraryName), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool CFStringGetCString(IntPtr theString, Span<byte> buffer, long bufferSize, uint encoding);
}
