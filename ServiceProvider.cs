using System;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using Jab;
using Serilog;
using ShadUI;
using Vid2Audio.ViewModels;

namespace Vid2Audio;

[ServiceProvider]
[Transient<MainWindowViewModel>]
[Transient<SearchViewModel>]
[Singleton<DialogManager>]
[Singleton<ToastManager>]
[Singleton<IMessenger, WeakReferenceMessenger>]
[Singleton(typeof(ILogger), Factory = nameof(LoggerFactory))]
[Singleton(typeof(PageManager), Factory = nameof(PageManagerFactory))]
public partial class ServiceProvider
{
    public static ILogger LoggerFactory()
    {
        var currentFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AHON_TRACK\\logs");

        Directory.CreateDirectory(currentFolder);

        var file = Path.Combine(currentFolder, "log.txt");

        var config = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(file, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Logger = config;

        return config;
    }

    public PageManager PageManagerFactory()
    {
        return new PageManager(this);
    }
}