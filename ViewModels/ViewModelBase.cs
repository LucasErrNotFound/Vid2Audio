using CommunityToolkit.Mvvm.ComponentModel;
using ShadUI;
using Vid2Audio.Services.Interface;

namespace Vid2Audio.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    protected DialogManager? DialogManager { get; init; }
    protected ToastManager? ToastManager { get; init; }
    protected PageManager? PageManager { get; init; }
    protected IVideoService? VideoService { get; init; }

    protected ViewModelBase() { }

    protected ViewModelBase(
        DialogManager? dialogManager = null,
        ToastManager? toastManager = null,
        PageManager? pageManager = null,
        IVideoService? videoService = null)
    {
        DialogManager = dialogManager;
        ToastManager = toastManager;
        PageManager = pageManager;
        VideoService = videoService;
    }
}