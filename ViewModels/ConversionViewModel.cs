using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HotAvalonia;
using ShadUI;
using Vid2Audio.Services;
using Vid2Audio.VideoConverter.Youtube;

namespace Vid2Audio.ViewModels;

[Page("conversion-view")]
public partial class ConversionViewModel : ViewModelBase, INavigable, INotifyPropertyChanged
{
    [ObservableProperty] 
    private bool _isSearchingVideo;
    
    private string _videoLink = string.Empty;
    private ObservableCollection<VideoItem> _videoList = VideoManager.VideoList;
    
    private readonly DialogManager _dialogManager;
    private readonly ToastManager _toastManager;
    private readonly PageManager _pageManager;

    public ConversionViewModel(DialogManager dialogManager, ToastManager toastManager, PageManager pageManager)
    {
        _dialogManager = dialogManager;
        _toastManager = toastManager;
        _pageManager = pageManager;
    }

    public ConversionViewModel()
    {
        _dialogManager =  new DialogManager();
        _toastManager = new ToastManager();
        _pageManager = new PageManager(new ServiceProvider());
    }
    
    public string VideoLink
    {
        get => _videoLink;
        set => SetProperty(ref _videoLink, value);
    }

    public ObservableCollection<VideoItem> VideoList
    {
        get => _videoList;
        set
        {
            _videoList = value;
            OnPropertyChanged();
        }
    }

    [AvaloniaHotReload]
    public void Initialize()
    {
    }
    
    [RelayCommand]
    private async Task DetectEnter()
    {
        var searchToastMessage = _toastManager.CreateToast(string.IsNullOrWhiteSpace(VideoLink)
                ? "No Video Link Provided"
                : "Link Detected")
            .WithContent(string.IsNullOrWhiteSpace(VideoLink)
                ? "Please enter the video link."
                : $"Link: {VideoLink}")
            .DismissOnClick();
        if (string.IsNullOrWhiteSpace(VideoLink))
            searchToastMessage.ShowError();
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

                {
                    var videoItem = new VideoItem
                    {
                        VideoTitle = videoData?.Title ?? "No title",
                        VideoUploader = videoData?.Uploader ?? "No uploader",
                        VideoThumbnail = videoData?.Thumbnail ?? "No thumbnail"
                    };
                    VideoManager.AddVideo(videoItem);

                    _toastManager.CreateToast("Video Added Successfully")
                        .WithContent($"Added: {videoItem.VideoTitle}")
                        .DismissOnClick()
                        .ShowSuccess();

                    VideoLink = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _toastManager.CreateToast("Error Processing Video")
                    .WithContent($"Failed to process video: {ex.Message}")
                    .DismissOnClick()
                    .ShowError();
            }
            finally
            {
                IsSearchingVideo = false;
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

public class VideoItem
{
    public string VideoTitle { get; set; } = string.Empty;
    public string VideoUploader { get; set; } = string.Empty; 
    public string VideoThumbnail { get; set; } = string.Empty;
}