#if LINUX
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using IrdLibraryClient;
using Tmds.DBus.Protocol;
using UDisks2.DBus;
using UI.Avalonia.ViewModels;

namespace UI.Avalonia.Views;

using DBusData = Dictionary<string, Dictionary<string, VariantValue>>;

public partial class MainWindow
{
    private Thread? monitorThread;
    private bool closing;
    
    partial void OnLoadedPlatform()
    {
        if (!OperatingSystem.IsLinux())
            return;
        
#pragma warning disable CA1416
        monitorThread = new(() => MonitorDriveEvents().Wait());
#pragma warning restore CA1416
        monitorThread.Start();
    }

    partial void OnClosingPlatform()
    {
        if (!OperatingSystem.IsLinux())
            return;
        
        closing = true;
    }

    [SupportedOSPlatform("linux")]
    private async Task MonitorDriveEvents()
    {
        try
        {
            if (DBusAddress.System is not {Length: >0} systemBusAddress)
            {
                Log.Warn("Failed to get system bus address for DBus");
                return;
            }

            var connection = new DBusConnection(new DBusConnectionOptions(systemBusAddress) { AutoConnect = false });
            await connection.ConnectAsync().ConfigureAwait(false);
            Log.Info("Connected to DBus");
            var objManager = new ObjectManager(connection, "org.freedesktop.UDisks2", "/org/freedesktop/UDisks2");
            using var addedWatcher = await objManager.WatchInterfacesAddedAsync(OnInterfaceAdded, ObserverFlags.None).ConfigureAwait(false);
            using var removedWatcher = await objManager.WatchInterfacesRemovedAsync(OnInterfaceRemoved, ObserverFlags.None).ConfigureAwait(false);
            while (!closing)
            {
                Thread.Yield();
                await Task.Yield();
                await Task.Delay(100).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to communicate with DBus");
        }
    }

    private void OnInterfaceAdded(Notification<(ObjectPath ObjectPath, DBusData InterfacesAndProperties)> n)
    {
        if (n.IsCompletion)
        {
            Log.Warn(n.Exception, "DBus error on InterfaceAdded signal");
            return;
        }

        var data = n.Value;
        if (data.InterfacesAndProperties.TryGetValue("org.freedesktop.UDisks2.Job", out var jobProps)
            && jobProps.TryGetValue("Operation", out var operation)
            && operation.Type is VariantValueType.String
            && operation.GetString() is { Length: >0 } operationValue and ("filesystem-mount" or "filesystem-unmount")
            && jobProps.TryGetValue("Objects", out var objList)
            && objList.Type is VariantValueType.Array
            && objList.ItemType is VariantValueType.ObjectPath
            && objList.GetArray<ObjectPath>() is { Length: > 0 } objPathList)
        {
            var opticalDriveList = objPathList
                .Select(p => p.ToString())
                .Where(p => p is { Length: > 0 } && p.StartsWith("/org/freedesktop/UDisks2/block_devices/sr"))
                .Select(Path.GetFileName)
                .Where(n => n is { Length: > 0 })
                .ToList();
            if (opticalDriveList.Count is 0)
                return;

            if (operationValue is "filesystem-mount")
            {
                Thread.Sleep(100);
                Thread.Yield();
                Dispatcher.UIThread.Post(() =>
                {
                    if (DataContext is MainWindowViewModel
                        {
                            CurrentPage: MainViewModel
                            {
                                DumpingInProgress: false
                            } vm and not
                            {
                                // still scanning
                                FoundDisc: true,
                                DumperIsReady: false
                            }
                        })
                    {
                        Log.Debug($"Received {operationValue} event with optical disc object, trying to scan disc drives… ({nameof(vm.DumpingInProgress)}: {vm.DumpingInProgress}, {nameof(vm.FoundDisc)}: {vm.FoundDisc}, {nameof(vm.DumperIsReady)}: {vm.DumperIsReady})");
                        vm.ScanDiscsCommand.Execute(null);
                    }
                }, DispatcherPriority.Background);
            }
            else if (operationValue is "filesystem-unmount")
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (DataContext is MainWindowViewModel
                        {
                            CurrentPage: MainViewModel
                            {
                                LastSelectedPhysicalDevice: {Length: >0} spd
                            } vm
                        }
                        && spd == $"/dev/{opticalDriveList[0]}")
                    {
                        Log.Debug($"Received {operationValue} event with optical disc object, trying to reset the state… ({nameof(vm.DumpingInProgress)}: {vm.DumpingInProgress}, {nameof(vm.FoundDisc)}: {vm.FoundDisc}, {nameof(vm.DumperIsReady)}: {vm.DumperIsReady})");
                        vm.Cts.Cancel();
                        vm.ResetViewModelCommand.Execute(null);
                    }
                }, DispatcherPriority.Background);
            }
        }
    }
    
    private void OnInterfaceRemoved(Notification<(ObjectPath ObjectPath, string[] Interfaces)> n)
    {
        if (n.IsCompletion)
        {
            Log.Warn(n.Exception, "DBus error on InterfaceRemoved signal");
            return;
        }

        var data = n.Value;
        if (data.ObjectPath.ToString() is { Length: > 0 } devicePath
            && devicePath.StartsWith("/org/freedesktop/UDisks2/block_devices/sr")
            && Path.GetFileName(devicePath) is { Length: > 0 } deviceName)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (DataContext is MainWindowViewModel
                    {
                        CurrentPage: MainViewModel
                        {
                            LastSelectedPhysicalDevice: {Length: >0} spd
                        } vm
                    }
                    && spd == $"/dev/{deviceName}")
                {
                    Log.Debug($"Received InterfaceRemoved signal with optical disc object, trying to reset the state… ({nameof(vm.DumpingInProgress)}: {vm.DumpingInProgress}, {nameof(vm.FoundDisc)}: {vm.FoundDisc}, {nameof(vm.DumperIsReady)}: {vm.DumperIsReady})");
                    vm.Cts.Cancel();
                    vm.ResetViewModelCommand.Execute(null);
                }
            }, DispatcherPriority.Background);
        }
    }
}
#endif