using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HotAvalonia;
using ShadUI;
using Vid2Audio.Components.ViewModels;

namespace Vid2Audio.ViewModels;

[Page("main-window")]
public partial class MainWindowViewModel : ViewModelBase, INavigable, INotifyPropertyChanged
{
    private readonly PageManager _pageManager;
    private readonly SearchViewModel _searchViewModel;
    private readonly ConversionViewModel  _conversionViewModel;
    private readonly SettingsDialogViewModel  _settingsDialogViewModel;
    
    [ObservableProperty]
    private DialogManager _dialogManager;

    [ObservableProperty]
    private ToastManager _toastManager;

    [ObservableProperty]
    private object? _selectedPage;
    
    [ObservableProperty]
    private string _currentRoute = "search-view";

    public MainWindowViewModel(
        DialogManager dialogManager, ToastManager toastManager, PageManager pageManager, SearchViewModel searchViewModel,  
        ConversionViewModel conversionViewModel,  SettingsDialogViewModel settingsDialogViewModel)
    {
        _dialogManager = dialogManager;
        _toastManager = toastManager;
        _pageManager = pageManager;
        _searchViewModel = searchViewModel;
        _conversionViewModel = conversionViewModel;
        _settingsDialogViewModel = settingsDialogViewModel;

        _pageManager.OnNavigate = SwitchPage;
    }
    
    public MainWindowViewModel()
    {
        _dialogManager = new DialogManager();
        _toastManager = new ToastManager();
        _pageManager = new PageManager(new ServiceProvider());
        _searchViewModel = new SearchViewModel();
        _conversionViewModel = new ConversionViewModel();
        _settingsDialogViewModel = new SettingsDialogViewModel();
    }

    [AvaloniaHotReload]
    public void Initialize()
    {
        SwitchPage(_searchViewModel);
    }
    
    private void SwitchPage(INavigable page, string route = "")
    {
        try
        {
            var pageType = page.GetType();
            if (string.IsNullOrEmpty(route)) route = pageType.GetCustomAttribute<PageAttribute>()?.Route ?? "search-view";
            CurrentRoute = route;

            if (SelectedPage == page) return;
            SelectedPage = page;
            CurrentRoute = route;
            page.Initialize();
            Debug.WriteLine("Success!");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error switching page: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenSettingsDialog()
    {
        _settingsDialogViewModel.Initialize();
        DialogManager.CreateDialog(_settingsDialogViewModel)
            .WithSuccessCallback(_ =>
                ToastManager.CreateToast("Settings Saved")
                    .WithContent("Your preferences were updated.")
                    .DismissOnClick()
                    .ShowSuccess())
            .WithCancelCallback(() =>
                ToastManager.CreateToast("Changes Discarded")
                    .WithContent("Unsaved edits were discarded.")
                    .DismissOnClick()
                    .ShowWarning()).WithMaxWidth(600)
            .Dismissible()
            .Show();
    }
    
    [RelayCommand]
    private void TryClose()
    {
        DialogManager.CreateDialog("Close Application", "Do you really want to exit the application?")
            .WithPrimaryButton("Yes", OnAcceptExit, DialogButtonStyle.Destructive)
            .WithCancelButton("No")
            .WithMinWidth(300)
            .Show();
    }

    private void OnAcceptExit() => Environment.Exit(0);
    
    [RelayCommand]
    private void OpenSearch() => SwitchPage(_searchViewModel);
}