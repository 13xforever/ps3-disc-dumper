using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using UI.Avalonia.ViewModels;

namespace UI.Avalonia;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var vmName = data?.GetType().FullName!;
        if (!vmName.EndsWith("ViewModel"))
            return new TextBlock { Text = "Not a ViewModel: " + vmName };
            
        var name = vmName.Replace("ViewModel", "View");
        if (name is {Length: >0} && Type.GetType(name) is Type viewType)
            return (Control)Activator.CreateInstance(viewType)!;
        
        name = name[..^4];
        if (name is {Length: >0} && Type.GetType(name) is Type type)
            return (Control)Activator.CreateInstance(type)!;
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}