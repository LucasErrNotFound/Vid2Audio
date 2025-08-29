using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Vid2Audio.ViewModels;

namespace Vid2Audio.Views;

public partial class SearchView : UserControl
{
    public SearchView()
    {
        InitializeComponent();
    }

    private async void OpenFileDialog_OnTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (DataContext is SearchViewModel viewModel)
            {
                await viewModel.OpenFileDialog();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}