using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HotAvalonia;
using ShadUI;
using Vid2Audio.Services;
using Vid2Audio.Services.Interface;
using Vid2Audio.VideoConverter.Youtube;

namespace Vid2Audio.ViewModels;

[Page("search-view")]
public partial class SearchViewModel : ViewModelBase, INavigable, INotifyPropertyChanged
{
    [ObservableProperty] 
    private bool _isSearchingVideo;

    private bool _isVideoLinkValid;
    private string _videoLink = string.Empty;
    
    private readonly DialogManager _dialogManager;
    private readonly ToastManager _toastManager;
    private readonly PageManager _pageManager;
    private readonly IVideoService _videoService;

    public SearchViewModel(DialogManager dialogManager, ToastManager toastManager, PageManager pageManager,  IVideoService videoService)
    {
        _dialogManager = dialogManager;
        _toastManager = toastManager;
        _pageManager = pageManager;
        _videoService = videoService;
    }
     
    public SearchViewModel()
    {
        _dialogManager = new DialogManager();
        _toastManager = new ToastManager();
        _pageManager = new PageManager(new ServiceProvider());
    }

    public string VideoLink
    {
        get => _videoLink;
        set => SetProperty(ref _videoLink, value);
    }

    [AvaloniaHotReload]
    public void Initialize()
    {
    }

    [RelayCommand]
    private async Task DetectEnter()
    {
        if (string.IsNullOrWhiteSpace(VideoLink))
        {
            _toastManager.CreateToast("No Video Link Provided")
                .WithContent("Please enter the video link.")
                .DismissOnClick()
                .ShowError();
        }
        else
        {
            IsSearchingVideo = true;

            try
            {
                _toastManager.CreateToast("Processing Video Link")
                    .WithContent("Fetching video metedata")
                    .DismissOnClick()
                    .ShowInfo();

                var videoData = await YoutubeConverter.GetVideoData(VideoLink);
                
                if (videoData == null)
                {
                    _toastManager.CreateToast("Failed to get video data")
                        .WithContent("Can't find video, please check the video link again.")
                        .DismissOnClick()
                        .ShowError();
                    _isVideoLinkValid = false;
                    return;
                }
                _isVideoLinkValid = true;
                
                {
                    var videoItem = new VideoItem(_videoService, _toastManager)
                    {
                        VideoTitle = videoData?.Title ?? "No title",
                        VideoUploader = videoData?.Uploader ?? "No uploader",
                        VideoThumbnail = videoData?.Thumbnail ?? "No thumbnail",
                        VideoUrl = videoData?.WebpageUrl ?? "No URL"
                    };
                    _videoService.AddVideo(videoItem);

                    _toastManager.CreateToast("Video Added Successfully")
                        .WithContent($"Added: {videoItem.VideoTitle}")
                        .DismissOnClick()
                        .ShowSuccess();

                    VideoLink = string.Empty;
                }
                
            }
            finally
            {
                IsSearchingVideo = false;
                if (_isVideoLinkValid) _pageManager.Navigate<ConversionViewModel>();
            }
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
            _toastManager.CreateToast("Video file selected")
                .WithContent($"{selectedFile.Name}")
                .DismissOnClick()
                .ShowInfo();
        }
    }
}