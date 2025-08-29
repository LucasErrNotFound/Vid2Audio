using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Vid2Audio.ViewModels;

namespace Vid2Audio;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<string, string> _componentMappings = new()
    {
        // Add mappings for your component
        // Pattern: "ViewModelName" -> "ComponentFolder"
        // {"ViewModelName", "Component Folder"}
    };

    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var viewModelType = param.GetType();
        var viewModelName = viewModelType.FullName!;

        var view = TryFindView(viewModelName, viewModelType);

        return view ?? new TextBlock { Text = "Not Found: " + viewModelName };
    }

    private Control? TryFindView(string viewModelName, Type viewModelType)
    {
        var viewModelClassName = viewModelType.Name;

        if (viewModelName.Contains("Components.ViewModels") &&
            _componentMappings.TryGetValue(viewModelClassName, out var componentFolder))
        {
            var componentViewName = viewModelName
                .Replace("Components.ViewModels", $"Components.{componentFolder}", StringComparison.Ordinal)
                .Replace("ViewModel", "View", StringComparison.Ordinal);

            var view = TryCreateView(componentViewName);
            if (view != null) return view;
        }
        var defaultViewName = viewModelName.Replace("ViewModel", "View", StringComparison.Ordinal);
        var view2 = TryCreateView(defaultViewName);
        return view2 ?? null;
    }

    private static Control? TryCreateView(string viewName)
    {
        try
        {
            var type = Type.GetType(viewName);
            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
        return null;
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}