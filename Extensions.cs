using ShadUI;
using Vid2Audio.Components.SettingsDialog;
using Vid2Audio.Components.ViewModels;

namespace Vid2Audio;

public static class Extensions
{
    public static ServiceProvider RegisterDialogs(this ServiceProvider service)
    {
        var dialogService = service.GetService<DialogManager>();
        dialogService.Register<SettingsDialogView, SettingsDialogViewModel>();

        return service;
    }
}