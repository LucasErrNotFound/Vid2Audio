using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HotAvalonia;
using ShadUI;
using Vid2Audio.Services;
using Vid2Audio.Services.Interface;

namespace Vid2Audio.ViewModels;

[Page("conversion-view")]
public partial class ConversionViewModel : VideoViewModelBase, INavigable 
{
    public ObservableCollection<VideoItem> VideoList => VideoService!.VideoList;

    public ConversionViewModel(
        DialogManager dialogManager, 
        ToastManager toastManager, 
        PageManager pageManager,  
        IVideoService videoService)
        : base(dialogManager, toastManager, pageManager, videoService)
    {
    }

    public ConversionViewModel()
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

public partial class VideoItem : ObservableObject
{
    private readonly ToastManager _toastManager;
    
    public VideoItem(IVideoService videoService, ToastManager toastManager)
    {
        _videoService = videoService;
        _toastManager = toastManager;
    }
    
    [ObservableProperty] 
    private string[] _audioFormatItems = ["MP3", "WAV", "FLAC", "M4A", "AAC"];
    
    [ObservableProperty]
    private string? _selectedAudioFormat = "MP3";
    
    [ObservableProperty]
    private bool _isDownloading;
    
    private readonly IVideoService _videoService;
    public string VideoTitle { get; set; } = string.Empty;
    public string VideoUploader { get; set; } = string.Empty; 
    public string VideoThumbnail { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;

    [RelayCommand]
    private void DeleteVideoItem() => _videoService.RemoveVideo(this);

    [RelayCommand]
    private async Task DownloadVideoItem()
    {
        _toastManager.CreateToast("Downloading, please wait...")
            .WithContent($"Downloading: {VideoTitle}")
            .DismissOnClick()
            .ShowInfo();
        
        var result = await _videoService.DownloadVideoAsync(this);
        var toast = _toastManager.CreateToast(result ? "Audio Download Successfully" : "Audio Download Failed")
            .WithContent($"Title: {VideoTitle}")
            .DismissOnClick();
        
        if (result) toast.ShowSuccess();
        else toast.ShowError();
    }
}