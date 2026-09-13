using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace RevivalSong.Views;

public partial class PrintCopiesDialog : Window
{
    public PrintCopiesDialog()
    {
        InitializeComponent();
    }

    private void OnPrintClicked(object? sender, RoutedEventArgs e)
    {
        int copies = CopiesCombo.SelectedIndex + 1;
        Close(copies);
    }
}