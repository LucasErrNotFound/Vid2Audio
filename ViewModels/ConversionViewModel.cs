using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security;
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
    [ObservableProperty]
    private bool _isAllSelected;
    
    [ObservableProperty]
    private bool _isDownloadAllVisible;
    
    [ObservableProperty]
    private bool _isClearAllVisible;

    [ObservableProperty]
    private bool _isCheckBoxVisible;
    
    [ObservableProperty]
    private string _selectAllButtonText = "Select All";
    
    public ObservableCollection<VideoItem> VideoList => VideoService!.VideoList;

    public ConversionViewModel(
        DialogManager dialogManager, 
        ToastManager toastManager, 
        PageManager pageManager,  
        IVideoService videoService)
        : base(dialogManager, toastManager, pageManager, videoService)
    {
        VideoList.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (VideoItem item in e.NewItems)
                {
                    item.PropertyChanged += VideoItem_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (VideoItem item in e.OldItems)
                {
                    item.PropertyChanged -= VideoItem_PropertyChanged;
                }
            }
            UpdateVisibilityStates();
            UpdateSelectAllState();
        };
        UpdateVisibilityStates();
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
    
    [RelayCommand]
    private void ToggleSelectAll()
    {
        foreach (var video in VideoList)
        {
            video.PropertyChanged -= VideoItem_PropertyChanged;
            video.IsVideoSelected = IsAllSelected;
            video.PropertyChanged += VideoItem_PropertyChanged;
        }
    
        SelectAllButtonText = IsAllSelected ? "Unselect All" : "Select All";
        UpdateVisibilityStates();
    }

    [RelayCommand]
    private void ShowVideoItemsDeletionDialog(VideoItem videoItem)
    {
        DialogManager!
            .CreateDialog(
                "Are you absolutely sure?",
                "This action cannot be undone. This will permanently clear all of your added video items.")
            .WithPrimaryButton("Continue", OnSubmitDeleteMultipleVideoItems, DialogButtonStyle.Destructive)
            .WithCancelButton("Cancel")
            .WithMaxWidth(512)
            .Dismissible()
            .Show();
    }
    
    [RelayCommand]
    private async Task DownloadMultipleVideos()
    {
        var selectedVideoItems = VideoList.Where(item => item.IsVideoSelected).ToList();
        if (selectedVideoItems.Count == 0) return;

        foreach (var item in selectedVideoItems)
        {
            item.IsDownloading = true;
        }

        ToastManager!.CreateToast("Downloading, please wait...")
            .WithContent($"Downloading {selectedVideoItems.Count} video(s)")
            .DismissOnClick()
            .ShowInfo();

        try
        {
            var result = await VideoService!.DownloadMultipleVideosAsync(selectedVideoItems);
        
            var toast = ToastManager!.CreateToast(result ? "Multiple Audio Download Successfully" : "Multiple Audio Download Failed")
                .WithContent($"Downloaded {selectedVideoItems.Count} video(s)")
                .DismissOnClick();

            if (result) 
                toast.ShowSuccess();
            else 
                toast.ShowError();
        }
        catch (Exception ex)
        {
            ToastManager!.CreateToast("Download Failed")
                .WithContent($"Error: {ex.Message}")
                .DismissOnClick()
                .ShowError();
        }
        finally
        {
            foreach (var item in selectedVideoItems)
            {
                item.IsDownloading = false;
            }
        }
    }

    private async Task OnSubmitDeleteMultipleVideoItems()
    {
        var selectedVideoItems = VideoList.Where(item => item.IsVideoSelected).ToList();
        if (selectedVideoItems.Count == 0) return;

        foreach (var item in selectedVideoItems)
        {
            await Task.Delay(100);
            VideoList.Remove(item);
        }
        
        IsAllSelected = false;
        SelectAllButtonText = "Select All";
        
        ToastManager!.CreateToast("Clear video items")
            .WithContent("Video items cleared successfully!")
            .DismissOnClick()
            .ShowSuccess();
    }
    
    private void UpdateVisibilityStates()
    {
        var count = VideoList.Count;
        var selectedCount = VideoList.Count(v => v.IsVideoSelected);
    
        IsCheckBoxVisible = count >= 2;
        IsDownloadAllVisible = count >= 2 && selectedCount >= 2;
        IsClearAllVisible = count >= 2 && selectedCount >= 2;
    }
    
    private void UpdateSelectAllState()
    {
        if (VideoList.Count == 0)
        {
            IsAllSelected = false;
            SelectAllButtonText = "Select All";
            return;
        }
    
        var allSelected = VideoList.All(v => v.IsVideoSelected);
        IsAllSelected = allSelected;
        SelectAllButtonText = allSelected ? "Unselect All" : "Select All";
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
    
    private void VideoItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(VideoItem.IsVideoSelected)) return;
        UpdateSelectAllState();
        UpdateVisibilityStates();
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
    
    [ObservableProperty]
    private bool _isVideoSelected;
    
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
        IsDownloading = true;
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
        
        IsDownloading = false;
    }
}