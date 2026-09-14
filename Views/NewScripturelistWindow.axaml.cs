using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RevivalSong.ViewModels;

namespace RevivalSong.Views;

public partial class NewScripturelistWindow : Window
{
    public NewScripturelistWindow()
    {
        InitializeComponent();

        var vm = new NewScripturelistViewModel();
        vm.CloseAction = result => Close(result);
        DataContext = vm;
    }
}