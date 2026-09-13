using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RevivalSong.ViewModels;

namespace RevivalSong.Views;

public partial class NewSonglistWindow : Window
{
    public NewSonglistWindow()
    {
        InitializeComponent();

        var vm = new NewSonglistViewModel();
        vm.CloseAction = result => Close(result);
        DataContext = vm;
    }
}