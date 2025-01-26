using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using UI.Avalonia.ViewModels;
using UI.Avalonia.Views;

namespace UI.Avalonia;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var vmType = data?.GetType();
        if (vmType == typeof(MainWindowViewModel))
            return new MainWindow();
        if (vmType == typeof(MainViewModel))
            return new Main();
        if (vmType == typeof(SettingsViewModel))
            return new Settings();
        if (vmType == typeof(ErrorStubViewModel))
            return new ErrorStub();

        var vmName = vmType?.FullName!;
        /*
        if (!vmName.EndsWith("ViewModel"))
            return new TextBlock { Text = "Not a ViewModel: " + vmName };

        var name = vmName.Replace("ViewModel", "View");
        if (name is {Length: >0} && Type.GetType(name) is Type viewType)
            return (Control)Activator.CreateInstance(viewType)!;

        name = name[..^4];
        if (name is {Length: >0} && Type.GetType(name) is Type type)
            return (Control)Activator.CreateInstance(type)!;
        */
        
        return new TextBlock { Text = "Couldn't fine view for " + vmName };
    }

    public bool Match(object? data) => data is ViewModelBase;
}