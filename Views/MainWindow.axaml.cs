using System.ComponentModel;
using Avalonia.Controls;
using Vid2Audio.ViewModels;
using Window = ShadUI.Window;

namespace Vid2Audio.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.TryCloseCommand.Execute(null);
        }
    }
}