using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using MicroCom.Runtime;
#pragma warning disable CS0108, CS8600

namespace UI.Avalonia.Utils.WinRTInterop.Impl;

internal unsafe partial class __MicroComIInspectableProxy : global::MicroCom.Runtime.MicroComProxyBase, IInspectable
{
    public void GetIids(ulong* iidCount, Guid** iids)
    {
        int __result;
        __result = (int)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*PPV)[base.VTableSize + 0])(PPV, iidCount, iids);
        if (__result != 0)
            throw new System.Runtime.InteropServices.COMException("GetIids failed", __result);
    }

    public IntPtr RuntimeClassName
    {
        get
        {
            int __result;
            IntPtr className = default;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*PPV)[base.VTableSize + 1])(PPV, &className);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("GetRuntimeClassName failed", __result);
            return className;
        }
    }

    public TrustLevel TrustLevel
    {
        get
        {
            int __result;
            TrustLevel trustLevel = default;
            __result = (int)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*PPV)[base.VTableSize + 2])(PPV, &trustLevel);
            if (__result != 0)
                throw new System.Runtime.InteropServices.COMException("GetTrustLevel failed", __result);
            return trustLevel;
        }
    }

    [System.Runtime.CompilerServices.ModuleInitializer()]
    internal static void __MicroComModuleInit()
    {
        global::MicroCom.Runtime.MicroComRuntime.Register(typeof(IInspectable), new Guid("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90"), (p, owns) => new __MicroComIInspectableProxy(p, owns));
    }

    protected __MicroComIInspectableProxy(IntPtr nativePointer, bool ownsHandle) : base(nativePointer, ownsHandle)
    {
    }

    protected override int VTableSize => base.VTableSize + 3;
}

unsafe class __MicroComIInspectableVTable : global::MicroCom.Runtime.MicroComVtblBase
{
    [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
    delegate int GetIidsDelegate(void* @this, ulong* iidCount, Guid** iids);
#if NET5_0_OR_GREATER
    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
    static int GetIids(void* @this, ulong* iidCount, Guid** iids)
    {
        IInspectable __target = null;
        try
        {
            {
                __target = (IInspectable)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                __target.GetIids(iidCount, iids);
            }
        }
        catch (System.Runtime.InteropServices.COMException __com_exception__)
        {
            return __com_exception__.ErrorCode;
        }
        catch (System.Exception __exception__)
        {
            global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
            return unchecked((int)0x80004005u);
        }

        return 0;
    }

    [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
    delegate int GetRuntimeClassNameDelegate(void* @this, IntPtr* className);
#if NET5_0_OR_GREATER
    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
    static int GetRuntimeClassName(void* @this, IntPtr* className)
    {
        IInspectable __target = null;
        try
        {
            {
                __target = (IInspectable)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                {
                    var __result = __target.RuntimeClassName;
                    *className = __result;
                }
            }
        }
        catch (System.Runtime.InteropServices.COMException __com_exception__)
        {
            return __com_exception__.ErrorCode;
        }
        catch (System.Exception __exception__)
        {
            global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
            return unchecked((int)0x80004005u);
        }

        return 0;
    }

    [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
    delegate int GetTrustLevelDelegate(void* @this, TrustLevel* trustLevel);
#if NET5_0_OR_GREATER
    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
    static int GetTrustLevel(void* @this, TrustLevel* trustLevel)
    {
        IInspectable __target = null;
        try
        {
            {
                __target = (IInspectable)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                {
                    var __result = __target.TrustLevel;
                    *trustLevel = __result;
                }
            }
        }
        catch (System.Runtime.InteropServices.COMException __com_exception__)
        {
            return __com_exception__.ErrorCode;
        }
        catch (System.Exception __exception__)
        {
            global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
            return unchecked((int)0x80004005u);
        }

        return 0;
    }

    protected __MicroComIInspectableVTable()
    {
#if NET5_0_OR_GREATER
        base.AddMethod((delegate* unmanaged[Stdcall]<void*, ulong*, Guid**, int>)&GetIids); 
#else
        base.AddMethod((GetIidsDelegate)GetIids); 
#endif
#if NET5_0_OR_GREATER
        base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr*, int>)&GetRuntimeClassName); 
#else
        base.AddMethod((GetRuntimeClassNameDelegate)GetRuntimeClassName); 
#endif
#if NET5_0_OR_GREATER
        base.AddMethod((delegate* unmanaged[Stdcall]<void*, TrustLevel*, int>)&GetTrustLevel); 
#else
        base.AddMethod((GetTrustLevelDelegate)GetTrustLevel); 
#endif
    }

    [System.Runtime.CompilerServices.ModuleInitializer()]
    internal static void __MicroComModuleInit() => global::MicroCom.Runtime.MicroComRuntime.RegisterVTable(typeof(IInspectable), new __MicroComIInspectableVTable().CreateVTable());
}

internal unsafe partial class __MicroComIActivationFactoryProxy : __MicroComIInspectableProxy, IActivationFactory
{
    public IntPtr ActivateInstance()
    {
        int __result;
        IntPtr instance = default;
        __result = (int)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*PPV)[base.VTableSize + 0])(PPV, &instance);
        if (__result != 0)
            throw new System.Runtime.InteropServices.COMException("ActivateInstance failed", __result);
        return instance;
    }

    [System.Runtime.CompilerServices.ModuleInitializer()]
    internal static void __MicroComModuleInit()
    {
        global::MicroCom.Runtime.MicroComRuntime.Register(typeof(IActivationFactory), new Guid("00000035-0000-0000-C000-000000000046"), (p, owns) => new __MicroComIActivationFactoryProxy(p, owns));
    }

    protected __MicroComIActivationFactoryProxy(IntPtr nativePointer, bool ownsHandle) : base(nativePointer, ownsHandle)
    {
    }

    protected override int VTableSize => base.VTableSize + 1;
}

unsafe class __MicroComIActivationFactoryVTable : __MicroComIInspectableVTable
{
    [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
    delegate int ActivateInstanceDelegate(void* @this, IntPtr* instance);
#if NET5_0_OR_GREATER
    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
    static int ActivateInstance(void* @this, IntPtr* instance)
    {
        IActivationFactory __target = null;
        try
        {
            {
                __target = (IActivationFactory)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                {
                    var __result = __target.ActivateInstance();
                    *instance = __result;
                }
            }
        }
        catch (System.Runtime.InteropServices.COMException __com_exception__)
        {
            return __com_exception__.ErrorCode;
        }
        catch (System.Exception __exception__)
        {
            global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
            return unchecked((int)0x80004005u);
        }

        return 0;
    }

    protected __MicroComIActivationFactoryVTable()
    {
#if NET5_0_OR_GREATER
        base.AddMethod((delegate* unmanaged[Stdcall]<void*, IntPtr*, int>)&ActivateInstance); 
#else
        base.AddMethod((ActivateInstanceDelegate)ActivateInstance); 
#endif
    }

    [System.Runtime.CompilerServices.ModuleInitializer()]
    internal static void __MicroComModuleInit() => global::MicroCom.Runtime.MicroComRuntime.RegisterVTable(typeof(IActivationFactory), new __MicroComIActivationFactoryVTable().CreateVTable());
}

internal unsafe partial class __MicroComIUISettings3Proxy : __MicroComIInspectableProxy, IUISettings3
{
    public WinRTColor GetColorValue(UIColorType desiredColor)
    {
        int __result;
        WinRTColor value = default;
        __result = (int)((delegate* unmanaged[Stdcall]<void*, UIColorType, void*, int>)(*PPV)[base.VTableSize + 0])(PPV, desiredColor, &value);
        if (__result != 0)
            throw new System.Runtime.InteropServices.COMException("GetColorValue failed", __result);
        return value;
    }

    [System.Runtime.CompilerServices.ModuleInitializer()]
    internal static void __MicroComModuleInit()
    {
        global::MicroCom.Runtime.MicroComRuntime.Register(typeof(IUISettings3), new Guid("03021BE4-5254-4781-8194-5168F7D06D7B"), (p, owns) => new __MicroComIUISettings3Proxy(p, owns));
    }

    protected __MicroComIUISettings3Proxy(IntPtr nativePointer, bool ownsHandle) : base(nativePointer, ownsHandle)
    {
    }

    protected override int VTableSize => base.VTableSize + 1;
}

unsafe class __MicroComIUISettings3VTable : __MicroComIInspectableVTable
{
    [System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)]
    delegate int GetColorValueDelegate(void* @this, UIColorType desiredColor, WinRTColor* value);
#if NET5_0_OR_GREATER
    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })] 
#endif
    static int GetColorValue(void* @this, UIColorType desiredColor, WinRTColor* value)
    {
        IUISettings3 __target = null;
        try
        {
            {
                __target = (IUISettings3)global::MicroCom.Runtime.MicroComRuntime.GetObjectFromCcw(new IntPtr(@this));
                {
                    var __result = __target.GetColorValue(desiredColor);
                    *value = __result;
                }
            }
        }
        catch (System.Runtime.InteropServices.COMException __com_exception__)
        {
            return __com_exception__.ErrorCode;
        }
        catch (System.Exception __exception__)
        {
            global::MicroCom.Runtime.MicroComRuntime.UnhandledException(__target, __exception__);
            return unchecked((int)0x80004005u);
        }

        return 0;
    }

    protected __MicroComIUISettings3VTable()
    {
#if NET5_0_OR_GREATER
        base.AddMethod((delegate* unmanaged[Stdcall]<void*, UIColorType, WinRTColor*, int>)&GetColorValue); 
#else
        base.AddMethod((GetColorValueDelegate)GetColorValue); 
#endif
    }

    [System.Runtime.CompilerServices.ModuleInitializer()]
    internal static void __MicroComModuleInit() => global::MicroCom.Runtime.MicroComRuntime.RegisterVTable(typeof(IUISettings3), new __MicroComIUISettings3VTable().CreateVTable());
}
