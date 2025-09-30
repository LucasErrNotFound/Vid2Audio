using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using HotAvalonia;
using ShadUI;
using Vid2Audio.Services;
using Vid2Audio.Services.Interface;

namespace Vid2Audio.ViewModels;

[Page("search-view")]
public partial class SearchViewModel : VideoViewModelBase, INavigable
{
    public SearchViewModel(
        DialogManager dialogManager, 
        ToastManager toastManager, 
        PageManager pageManager,  
        IVideoService videoService)
        : base(dialogManager, toastManager, pageManager, videoService)
    {
    }

    public SearchViewModel()
        : base(new DialogManager(), new ToastManager(), 
            new PageManager(new ServiceProvider()), 
            new VideoService { VideoList = [] })
    {
    }

    [AvaloniaHotReload]
    public new void Initialize()
    {
    }

    [RelayCommand]
    private async Task DetectEnter()
    {
        if (!ValidateVideoLink()) return;
        
        IsSearchingVideo = true;
        ShowProcessingNotification();

        try
        {
            var videoItem = await FetchAndCreateVideoItem();
            
            if (videoItem == null)
            {
                ShowVideoNotFoundError();
                return;
            }

            VideoService!.AddVideo(videoItem);
            ShowVideoAddedSuccess(videoItem);
            ClearInput();
            
            PageManager!.Navigate<ConversionViewModel>();
        }
        catch (Exception ex)
        {
            ShowProcessingError(ex);
        }
        finally
        {
            IsSearchingVideo = false;
        }
    }

    public async Task OpenFileDialog()
    {
        var topLevel = App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Video File",
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType("Video Files")
                {
                    Patterns = ["*.mp4", "*.mkv", "*.mov", "*.avi"]
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = ["*.*"]
                }
            ]
        });

        if (files.Count > 0)
        {
            var selectedFile = files[0];
            ToastManager?.CreateToast("Video file selected")
                .WithContent($"{selectedFile.Name}")
                .DismissOnClick()
                .ShowInfo();
        }
    }
}