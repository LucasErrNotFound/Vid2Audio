using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using HotAvalonia;
using ShadUI;
using Vid2Audio.VideoConverter.Youtube;

namespace Vid2Audio.ViewModels;

[Page("search-view")]
public partial class SearchViewModel : ViewModelBase, INavigable, INotifyPropertyChanged
{
    private string _videoLink = string.Empty;
    
    private readonly DialogManager _dialogManager;
    private readonly ToastManager _toastManager;
    private readonly PageManager _pageManager;

    public SearchViewModel(DialogManager dialogManager, ToastManager toastManager, PageManager pageManager)
    {
        _dialogManager = dialogManager;
        _toastManager = toastManager;
        _pageManager = pageManager;
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
            searchToastMessage.ShowInfo();
            _pageManager.Navigate<ConversionViewModel>();

            try
            {
                await YoutubeConverter.DownloadYoutube(VideoLink);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
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