using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ShadUI;
using Vid2Audio;
using Vid2Audio.Services.Interface;
using Vid2Audio.ViewModels;
using YoutubeDLSharp.Metadata;

public abstract partial class VideoViewModelBase : ViewModelBase, INavigable
{
    [ObservableProperty] 
    private bool _isSearchingVideo;

    private string _videoLink = string.Empty;

    protected VideoViewModelBase(
        DialogManager dialogManager, 
        ToastManager toastManager, 
        PageManager pageManager,  
        IVideoService videoService)
        : base(dialogManager, toastManager, pageManager, videoService)
    {
    }

    public string VideoLink
    {
        get => _videoLink;
        set => SetProperty(ref _videoLink, value);
    }

    public virtual void Initialize() { }

    // Shared validation
    protected bool ValidateVideoLink()
    {
        if (VideoService!.ValidateVideoUrl(VideoLink)) 
            return true;
            
        ToastManager!.CreateToast("Invalid Video Link")
            .WithContent("Please enter a valid video URL.")
            .DismissOnClick()
            .ShowError();
        return false;
    }

    // Shared video processing
    protected async Task<VideoItem?> FetchAndCreateVideoItem()
    {
        var videoData = await VideoService!.FetchVideoDataAsync(VideoLink);
        return videoData == null ? null : CreateVideoItemFromData(videoData);
    }

    protected VideoItem CreateVideoItemFromData(VideoData videoData)
    {
        return new VideoItem(VideoService!, ToastManager!)
        {
            VideoTitle = videoData.Title ?? "No title",
            VideoUploader = videoData.Uploader ?? "No uploader",
            VideoThumbnail = videoData.Thumbnail ?? "No thumbnail",
            VideoUrl = videoData.WebpageUrl ?? "No URL"
        };
    }

    // Shared notification methods
    protected void ShowProcessingNotification()
    {
        ToastManager!.CreateToast("Processing Video Link")
            .WithContent("Fetching video metadata")
            .DismissOnClick()
            .ShowInfo();
    }

    protected void ShowVideoNotFoundError()
    {
        ToastManager!.CreateToast("Failed to Get Video Data")
            .WithContent("Can't find video, please check the video link again.")
            .DismissOnClick()
            .ShowError();
    }

    protected void ShowProcessingError(Exception ex)
    {
        ToastManager!.CreateToast("Error Processing Video")
            .WithContent($"Failed to process video: {ex.Message}")
            .DismissOnClick()
            .ShowError();
    }

    protected void ShowVideoAddedSuccess(VideoItem videoItem)
    {
        ToastManager!.CreateToast("Video Added Successfully")
            .WithContent($"Added: {videoItem.VideoTitle}")
            .DismissOnClick()
            .ShowSuccess();
    }

    protected void ClearInput()
    {
        VideoLink = string.Empty;
    }
}