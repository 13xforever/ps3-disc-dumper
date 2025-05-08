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

    /*
     * $ dbus-monitor --system "type='signal',interface='org.freedesktop.DBus.ObjectManager'"
     *
     * signal time=1746716823.547392 sender=:1.7 -> destination=(null destination) serial=295 path=/org/freedesktop/UDisks2; interface=org.freedesktop.DBus.ObjectManager; member=InterfacesAdded
     *   object path "/org/freedesktop/UDisks2/jobs/3"
     *   array [
     *      dict entry(
     *         string "org.freedesktop.UDisks2.Job"
     *         array [
     *            dict entry(
     *               string "Operation"
     *               variant                   string "filesystem-mount"
     *            )
     *            dict entry(
     *               string "Progress"
     *               variant                   double 0
     *            )
     *            dict entry(
     *               string "ProgressValid"
     *               variant                   boolean false
     *            )
     *            dict entry(
     *               string "Bytes"
     *               variant                   uint64 0
     *            )
     *            dict entry(
     *               string "Rate"
     *               variant                   uint64 0
     *            )
     *            dict entry(
     *               string "StartTime"
     *               variant                   uint64 1746716823546146
     *            )
     *            dict entry(
     *               string "ExpectedEndTime"
     *               variant                   uint64 0
     *            )
     *            dict entry(
     *               string "Objects"
     *               variant                   array [
     *                     object path "/org/freedesktop/UDisks2/block_devices/sr0"
     *                  ]
     *            )
     *            dict entry(
     *               string "StartedByUID"
     *               variant                   uint32 0
     *            )
     *            dict entry(
     *               string "Cancelable"
     *               variant                   boolean true
     *            )
     *         ]
     *      )
     *   ]
     *
     * signal time=1746716930.148190 sender=:1.7 -> destination=(null destination) serial=346 path=/org/freedesktop/UDisks2; interface=org.freedesktop.DBus.ObjectManager; member=InterfacesRemoved
     *   object path "/org/freedesktop/UDisks2/block_devices/sr0"
     *   array [
     *      string "org.freedesktop.UDisks2.Filesystem"
     *   ]
     */

    [SupportedOSPlatform("linux")]
    private async Task MonitorDriveEvents()
    {
        try
        {
            if (Address.System is not {Length: >0} systemBusAddress)
            {
                Log.Warn("Failed to get system bus address for DBus");
                return;
            }

            var connection = new Connection(systemBusAddress);
            await connection.ConnectAsync().ConfigureAwait(false);
            Log.Info("Connected to DBus");
            var uDisks2Service = new UDisks2Service(connection, "org.freedesktop.UDisks2");
            var objManager = new ObjectManager(uDisks2Service, "/org/freedesktop/UDisks2");
            using var addedWatcher = await objManager.WatchInterfacesAddedAsync(OnInterfaceAdded).ConfigureAwait(false);
            using var removedWatcher = await objManager.WatchInterfacesRemovedAsync(OnInterfaceRemoved).ConfigureAwait(false);
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

    private void OnInterfaceAdded(Exception? ex, (ObjectPath ObjectPath, DBusData InterfacesAndProperties) data)
    {
        if (ex is not null)
        {
            Log.Warn(ex, "DBus error on InterfaceAdded signal");
            return;
        }

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
                                dumper: { SelectedPhysicalDevice: { Length: > 0 } spd } dumper
                            } vm
                        }
                        && spd == $"/dev/{opticalDriveList[0]}")
                    {
                        Log.Debug($"Received {operationValue} event with optical disc object, trying to reset the state… ({nameof(vm.DumpingInProgress)}: {vm.DumpingInProgress}, {nameof(vm.FoundDisc)}: {vm.FoundDisc}, {nameof(vm.DumperIsReady)}: {vm.DumperIsReady})");
                        dumper.Cts.Cancel();
                        vm.ResetViewModelCommand.Execute(null);
                    }
                }, DispatcherPriority.Background);
            }
        }
    }
    
    private void OnInterfaceRemoved(Exception? ex, (ObjectPath ObjectPath, string[] Interfaces) data)
    {
        if (ex is not null)
        {
            Log.Warn(ex, "DBus error on InterfaceRemoved signal");
            return;
        }

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
                            dumper: { SelectedPhysicalDevice: { Length: > 0 } spd } dumper
                        } vm
                    }
                    && spd == $"/dev/{deviceName}")
                {
                    Log.Debug($"Received InterfaceRemoved signal with optical disc object, trying to reset the state… ({nameof(vm.DumpingInProgress)}: {vm.DumpingInProgress}, {nameof(vm.FoundDisc)}: {vm.FoundDisc}, {nameof(vm.DumperIsReady)}: {vm.DumperIsReady})");
                    dumper.Cts.Cancel();
                    vm.ResetViewModelCommand.Execute(null);
                }
            }, DispatcherPriority.Background);
        }
    }
}
#endif