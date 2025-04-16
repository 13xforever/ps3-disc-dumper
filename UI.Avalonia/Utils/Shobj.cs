#if WINDOWS
using System;
using TerraFX.Interop.Windows;
using System.Runtime.InteropServices;
using static TerraFX.Interop.Windows.Windows;

namespace UI.Avalonia.Utils;

internal sealed unsafe class Shobj: IDisposable
{
    private ComPtr<ITaskbarList3> taskbar;

    public ITaskbarList3* Taskbar => taskbar.Get();
    
    public Shobj()
    {
        fixed (ITaskbarList3** ptr = taskbar)
        {
            var hr = CoCreateInstance(
                __uuidof<TaskbarList>(),
                null,
                (uint)CLSCTX.CLSCTX_INPROC_SERVER,
                __uuidof<ITaskbarList3>(),
                (void**)ptr
            );
            if (hr.FAILED)
                Marshal.ThrowExceptionForHR(hr.Value);
        }
    }

    ~Shobj()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
            taskbar.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
#endif