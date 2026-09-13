using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RevivalSong.ViewModels;

namespace RevivalSong.Views;

public partial class NewSongWindow : Window
{
    public NewSongWindow(int? songId = null)
    {
        InitializeComponent();

        DataContext = new NewSongViewModel(this, songId);
    }
}