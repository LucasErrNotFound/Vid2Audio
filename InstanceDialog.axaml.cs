using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Window = ShadUI.Window;

namespace Vid2Audio;

public partial class InstanceDialog : Window
{
    public InstanceDialog()
    {
        InitializeComponent();
    }
    
    private void OnClose(object sender, RoutedEventArgs e) => Close();
}