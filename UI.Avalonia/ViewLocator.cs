using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using UI.Avalonia.ViewModels;

namespace UI.Avalonia;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var name = data?.GetType().FullName!.Replace("ViewModel", "View");
        if (name is {Length: >0} && Type.GetType(name) is Type type)
            return (Control)Activator.CreateInstance(type)!;
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}