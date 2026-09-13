namespace RevivalSong.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public partial class DisplayItemModel : ObservableObject
{
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private bool _isPrimary;
    [ObservableProperty] private string _resolution = "";

    public int ScreenIndex { get; set; }
}