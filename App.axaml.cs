using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading;
using Avalonia.Markup.Xaml;
using Vid2Audio.ViewModels;
using Vid2Audio.Views;

namespace Vid2Audio;

public partial class App : Application
{
    private static Mutex? _appMutex;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
        
        _appMutex = new Mutex(true, "Vid2AudioSingleInstanceMutex", out var createdNew);
        if (!createdNew)
        {
            var instanceDialog = new InstanceDialog();
            instanceDialog.Show();
            return;
        }
        
        DisableAvaloniaDataAnnotationValidation();
        var provider = new ServiceProvider();
        
        var viewModel = provider.GetService<MainWindowViewModel>();
        viewModel.Initialize();

        var mainWindow = new MainWindow { DataContext = viewModel };

        desktop.MainWindow = mainWindow;
        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}